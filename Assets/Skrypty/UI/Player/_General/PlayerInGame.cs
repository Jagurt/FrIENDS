#pragma warning disable CS0618 // Type is too old

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEditor;

public class PlayerInGame : NetworkBehaviour
{
    static CustomNetworkManager CustomNetworkManager;
    // Reference to ServerGameManager script for easier use in this script.
    private ServerGameManager serverGameManager;
    // Static reference to PlayerInGame(this) script, it gets set by the object that is meant to be localPlayer with authority.
    internal static PlayerInGame localPlayerInGame;
    // For checking if initial variables were set for this object on server.
    [SyncVar] bool started = false;

    [SyncVar] internal string NickName;                         // Nick name of player got from options in title screen.
    internal Sprite Avatar;                                     // Not Implemented Yet

    //      Stats      //
    [SyncVar] [SerializeField] private short level = 1;         // Level of a player
    public short Level
    {
        get => level;
        set
        {
            level = value;
            StartCoroutine(ServerUpdateLevelUI());  // Updating Level in UI on change
        }
    }
    [SyncVar] private short bigItemsLimit;                      // Describes how many "Big" items can player use at once, Not Implemented Yet.
    [SyncVar] private short ownedCardsLimit;                    // Limits maximum number of cards player can own at end of their turn.
    [SyncVar] private short luck;                               // Modifies random outcomes of cards effects, Not Implemented Yet
    [SyncVar] internal bool isAlive;                            // Player cannot perform any actions until ressurected, Not Implemented Yet
    [SyncVar] [SerializeField] internal bool hasTurn = false;   // Certain actions are only available when player has turn.

    //      UI      //
    /// <summary>
    /// Variable to store coroutine for initializing opponentInPanel object in enemiesPanel.
    /// This initialization is run first on all PlayerInGame objects by default.
    /// It is meant to finish only on PlayerInGame objects that local player DOESN'T have authority over.
    /// So in case of having authority over PlayerInGame object we need to stop this coroutine before it finishes.
    /// To do that we store coroutine in variable below.
    /// </summary>
    IEnumerator initializeInEnemiesPanel;

    [SerializeField] internal GameObject opponentStatsUIPrefab;         // Reference to opponentInPanel Prefab.
    internal GameObject playerStatsUI;                                  // Reference to playerInPanel.
    internal GameObject opponentStatsUI;                                // Reference to opponentInPanel. 
    [SerializeField] internal static GameObject playerCanvas;           // Reference to playerCanvas.

    internal Transform handContent;                                     // Reference to players "Hand" Transform that stores players owned cards.
    private static Transform opponentsStatsUIPanel;                     // Reference to panels Transform that stores objects ( OpponentInPanel ) which show information about enemies
    [SerializeField] internal Transform equipment;                      // Reference to Transform that stores slots for eqipment.
    internal ProgressButton progressButton;                             // Reference to ProgressButton script for easier access to its methods.

    //      Equipment       //
    [SerializeField] internal List<GameObject> equippedItems = new List<GameObject>();
    [SerializeField] List<GameObject> activeBuffs = new List<GameObject>();

    [SerializeField] internal GameObject storedObject = null;           // Reference to selected card, to choose target for it.

    void Start()
    {
        CustomNetworkManager = CustomNetworkManager.customNetworkManager;
        serverGameManager = ServerGameManager.serverGameManager;

        handContent = transform;
        initializeInEnemiesPanel = InitializeOpponentStatsUI();
        StartCoroutine(initializeInEnemiesPanel);

        ClientInitialize();
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        ClientInitialize();
    }

    public override void OnStopAuthority()
    {
        base.OnStopAuthority();
        ClientInitialize();
    }

    private void ClientInitialize()
    {
        //Debug.Log("ClientInitialize(): hasAuthority - " + hasAuthority);
        // Check if player "Owns" this object, this is set in "PlayerManager" script by "SpawnWithClientAuthority" method.
        if (hasAuthority)
        {
            if (initializeInEnemiesPanel != null)
                StopCoroutine(initializeInEnemiesPanel);
            if (!localPlayerInGame)
                localPlayerInGame = this;

            CmdStart(NickName);

            // Initializing UI objects
            if (PlayerCanvas.playerCanvas)
                playerCanvas = PlayerCanvas.playerCanvas.gameObject;
            TradePanel.Initialize();
            LevelCounter.Initialize();
            SaveGameMenu.Initialize();
            AcceptTradeButton.Initialize();

            //      Setting references for UI Objects      //
            progressButton = playerCanvas.transform.Find("ProgressButton").GetComponent<ProgressButton>();
            /// Since I call this method in OnStartAuthority and OnStopAuthority I need to check if I have not already created opponentStatsUI.
            /// This is relevant only for Host upon disconnection of player owning this object.
            /// When owner of this player disconnects authority over this object is passed to host, so this method is called in OnStartAuthority for host.
            /// I don't want to overwrite stats of this object to hosts "playerStatsUI" so I need to check 
            /// if "opponentStatsUI" for this object has been created.
            if (opponentStatsUI == null)
            {
                playerStatsUI = playerCanvas.transform.Find("PlayerInPanel").gameObject;
                playerStatsUI.GetComponent<OpponentInPanel>().Initialize(this);
                equipment = playerStatsUI.transform.Find("Eq");
                handContent = playerCanvas.transform.Find("HandPanel").Find("Content");
            }
        }
        serverGameManager = ServerGameManager.serverGameManager;
    }
    /// <summary>
    /// Coroutine for initializing enemy stats UI in enemies panel.
    /// </summary>
    IEnumerator InitializeOpponentStatsUI()
    {
        // Waiting until required references are set by the local player.
        while (!localPlayerInGame || !playerCanvas)
        {
            Debug.Log("Waiting for localPlayerInGame");
            yield return new WaitForSeconds(0.25f);
        }

        // Initializing enemy panel ui object for local player     
        opponentsStatsUIPanel = playerCanvas.transform.Find("OpponentsPanel");
        if (opponentStatsUI != null || playerStatsUI != null) yield break;
        opponentStatsUI = Instantiate(opponentStatsUIPrefab);
        opponentStatsUI.transform.SetParent(opponentsStatsUIPanel);
        opponentStatsUI.GetComponent<OpponentInPanel>().Initialize(this);
        equipment = opponentStatsUI.transform.Find("EnemyEq");
    }
    /// <summary>
    /// [Command] - Called BY the CIENTS, run ON SERVER. Can be run only by objects with local authority.
    /// Initializes variables (for this object) on the server that are then set on clients
    /// </summary>
    [Command]
    void CmdStart( string NickName )
    {
        if (started)
            return;

        started = true;
        StartCoroutine(ServerStart(NickName));
    }
    /// <summary>
    /// Called and run only on server.
    /// Initializes variables (for this object) on the server that are then set on clients
    /// </summary>
    [Server]
    IEnumerator ServerStart( string NickName )
    {
        bigItemsLimit = 1;
        ownedCardsLimit = 5;
        luck = 0;
        isAlive = true;

        yield return new WaitUntil(() => serverGameManager != null);

        StartCoroutine(serverGameManager.ReportPresence());
        this.NickName = NickName;
    }
    /// <summary>
    /// [Command] - Called BY the CIENTS, run ON SERVER. Can be run only by objects with local authority.
    /// Method called when ProgressButton is pressed, calls ReadyPlayersUp on Server.
    /// </summary>
    [Command]
    internal void CmdReadyPlayersUp()
    {
        serverGameManager.ReadyPlayersUp();
    }
    /// <summary>
    /// [Server] - Called and run only on server.
    /// Called by When game is entered when save is loaded, and used to equip loaded items.
    /// </summary>
    /// <param name="card"> Card to equip by this player. </param>
    [Server]
    internal void ServerOnLoadEquip( GameObject card )
    {
        NetworkInstanceId cardNetId = card.GetComponent<Card>().GetNetId();
        RpcOnLoadEquip(cardNetId);
    }
    /// <summary>
    /// [Server] - Called and run only on server.
    /// Called by When player is reconnected, and used to reequip items to sync them in reconnected players UI.
    /// </summary>
    /// <param name="card"> Card to reequip by reconnected player. </param>
    [Server]
    internal IEnumerator ServerOnReconnectEquip( GameObject card )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        NetworkInstanceId cardNetId = card.GetComponent<Card>().GetNetId();
        RpcOnLoadEquip(cardNetId);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }
    /// <summary>
    /// [Client] - Called and run only on clients.
    /// Used to show eqipping item in UI.
    /// </summary>
    /// <param name="card"></param>
    [Client]
    internal void ClientEquip( GameObject card )
    {
        if (hasAuthority && card.GetComponent<Draggable>().Placeholder)
            Destroy(card.GetComponent<Draggable>().Placeholder);

        card.GetComponent<Draggable>().enabled = true;
        card.GetComponent<Card>().ClientSetActiveCardButtons(false);
        serverGameManager.cardsUsageQueue.Remove(card);

        //Debug.Log("Equipping " + equipmentCard.cardValues.name + " in the " + equipmentCard.eqPart + " slot");
        if (!equippedItems.Contains(card))
            equippedItems.Add(card);                                            // player.equippedItems.Add(this.gameObject);                                                   

        EquipmentCard eqCardScript = card.GetComponent<EquipmentCard>();    // Setting item in item slot UI for players that don't own this item
        EqItemSlot eqSlot = equipment.GetChild((int)(eqCardScript.cardValues as EquipmentValue).eqPart).GetComponentInChildren<EqItemSlot>();

        eqSlot.ReceiveEq(card);
        StartCoroutine(ClientUpdateOwnedCardsUI());
        StartCoroutine(ClientUpdateLevelUI());
    }
    /// <summary>
    /// [ClientRpc] - Called only by server, executed only on clients.
    /// </summary>
    [ClientRpc]
    void RpcOnLoadEquip( NetworkInstanceId cardNetId ) // Calling equiping card on all clients
    {
        GameObject eqCard = ClientScene.FindLocalObject(cardNetId); // Finding card locally via its "NetworkInstanceId"
        StartCoroutine(ClientOnLoadEquip(eqCard));
    }
    /// <summary>
    /// [Client] - Called and run only on clients.
    /// Used when loading to set equipment in correct item slot in eq.
    /// </summary>
    /// <param name="eqCard"></param>
    [Client]
    IEnumerator ClientOnLoadEquip( GameObject eqCard )
    {
        // I use this upon reconnection too, so I need a check to prevent player having equipped duplicate of same item.
        // Players can't have equipped identical items by design anyway.
        if (!equippedItems.Contains(eqCard))
            equippedItems.Add(eqCard);

        // Setting item in item UI slot 
        EquipmentCard eqCardScript = eqCard.GetComponent<EquipmentCard>();
        // Upon reconnection, this Coroutine can be called before sync, so I need wait for required variable to be assigned.
        yield return new WaitUntil(() => equipment != null);
        equipment.GetChild((int)(eqCardScript.cardValues as EquipmentValue).eqPart).GetComponentInChildren<EqItemSlot>().ReceiveEq(eqCard);
    }
    /// <summary>
    /// Called in event when equipped item is being dragged of its eq item slot.
    /// Why not call CmdUnequip directly?
    /// Becouse [Command] methods can only be called from objects with authority, which is this(PlayerInGame) and PlayerManager.
    /// So if I want to call [Command] from other object(without authority) I need to call this method, and then from this method(with authority) I can call [Command] method.
    /// </summary>
    /// <param name="card"></param>
    internal void Unequip( GameObject card )
    {
        CmdUnequip(card.GetComponent<Card>().netId);
    }

    [Command]
    internal void CmdUnequip( NetworkInstanceId cardNetId )
    {
        StartCoroutine(ServerUnequip(cardNetId));
    }
    /// <summary>
    /// Level of unequipping item player is reduced accordingly to items level.
    /// </summary>
    /// <param name="cardNetId"></param>
    [Server]
    IEnumerator ServerUnequip( NetworkInstanceId cardNetId )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        GameObject card = ClientScene.FindLocalObject(cardNetId);
        //Debug.Log("Unequipping - " + card);

        Level -= card.GetComponent<EquipmentCard>().cardValues.level;

        yield return new WaitForEndOfFrame();

        RpcUnequip(cardNetId);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }

    [ClientRpc]
    internal void RpcUnequip( NetworkInstanceId cardNetId )
    {
        ClientUnequip(cardNetId);
    }
    /// <summary>
    /// Locally unequipping card and updating changes in UI.
    /// </summary>
    /// <param name="cardNetId"></param>
    [Client]
    internal void ClientUnequip( NetworkInstanceId cardNetId )
    {
        GameObject eqCard = ClientScene.FindLocalObject(cardNetId);

        EquipmentCard eqCardScript = eqCard.GetComponent<EquipmentCard>();
        EqItemSlot eqSlot = equipment.GetChild((int)(eqCardScript.cardValues as EquipmentValue).eqPart).GetComponentInChildren<EqItemSlot>();

        equippedItems.Remove(eqCard);

        if (!hasAuthority)
            eqSlot.ReturnEnemyEq();

        StartCoroutine(ClientUpdateOwnedCardsUI());
        StartCoroutine(ClientUpdateLevelUI());
    }
    /// <summary>
    /// Locally unequipping item, equipping new one and updating changes in UI.
    /// </summary>
    /// <param name="cardNetId"></param>
    [Client]
    internal void ClientSwitchEq( NetworkInstanceId cardNetId )
    {
        GameObject eqCard = ClientScene.FindLocalObject(cardNetId);

        EquipmentCard eqCardScript = eqCard.GetComponent<EquipmentCard>();
        EqItemSlot eqSlot = equipment.GetChild((int)(eqCardScript.cardValues as EquipmentValue).eqPart).GetComponentInChildren<EqItemSlot>();

        equippedItems.Remove(eqSlot.heldItem);

        if (!hasAuthority)
            eqSlot.ReturnEnemyEq();
        else
            eqSlot.ReturnEq();

        ClientEquip(eqCard);
    }

    private IEnumerator RequestCardsDrawCoroutine( Deck deck, int quantity, bool worksOnDrawingPlayer = false, bool choice = false )
    {
        for (int i = 0; i < quantity; i++)
        {
            if (CustomNetworkManager.isServerBusy)
                yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
            CustomNetworkManager.isServerBusy = true;

            CmdRequestCardDraw(deck, worksOnDrawingPlayer, choice);

            yield return new WaitForEndOfFrame();
            CustomNetworkManager.isServerBusy = false;
        }
    }

    [Command]
    private void CmdRequestCardDraw( Deck deck, bool worksOnDrawingPlayer, bool choice )
    {
        ServerSendCard(deck, worksOnDrawingPlayer, choice);
    }
    /// <summary> Calling ServerSendCards set number of times for set deck. </summary>
    /// <param name="deck"> Deck from which card is drawn. </param>
    /// <param name="quantity"> Number of cards to dra. </param>
    /// <param name="worksOnDrawingPlayer"> If drawn cards are applied to drawing player once they are drawn. </param>
    /// <param name="choice"> If player gets to choose which card to pick. </param>
    [Server]
    internal IEnumerator ServerSendCards( Deck deck, int quantity, bool worksOnDrawingPlayer = false, bool choice = false )
    {
        Debug.Log("ServerSendCardsCoroutine");
        for (int i = 0; i < quantity; i++)
        {
            if (CustomNetworkManager.isServerBusy)
                yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
            CustomNetworkManager.isServerBusy = true;

            ServerSendCard(deck, worksOnDrawingPlayer, choice);

            yield return new WaitForEndOfFrame();
            CustomNetworkManager.isServerBusy = false;
        }
    }
    /// <summary> Finding card from set deck and sending to player. </summary>
    /// <param name="deck"> Deck from which card is drawn. </param>
    /// <param name="worksOnDrawingPlayer"> If drawn cards are applied to drawing player once they are drawn. </param>
    /// <param name="choice"> If player gets to choose which card to pick. </param>
    [Server]
    private void ServerSendCard( Deck deck, bool worksOnDrawingPlayer = false, bool choice = false ) // Drawing card from deck and sending to Clients
    {
        int last = -1;
        GameObject drawnCard = null;
        DrawCardZone drawCardZone = null;

        switch (deck)
        {
            case Deck.Doors:
                // Draws a card, sets its parent to this player object
                drawCardZone = ServerGameManager.serverGameManager.ServerDecksManager.DoorsDeck.GetComponent<DrawCardZone>();
                last = ServerGameManager.serverGameManager.ServerDecksManager.Doors.Count - 1; // Number of cards in deck - 1
                break;
            case Deck.Treasures:
                drawCardZone = ServerGameManager.serverGameManager.ServerDecksManager.TreasuresDeck.GetComponent<DrawCardZone>();
                last = ServerGameManager.serverGameManager.ServerDecksManager.Treasures.Count - 1;
                break;
            case Deck.HelpingHand:
                break;
            case Deck.Spells:
                break;
            case Deck.DiscardedDoors:
                break;
            case Deck.DiscardedSpells:
                break;
            case Deck.DiscardedTreasures:
                break;
            case Deck.DiscardedHelpingHand:
                break;
            default:
                break;
        }

        if (last < 0)
        {
            Debug.Log("Drawing Card From Empty Deck!");
            return;
        }

        drawnCard = drawCardZone.DrawCard(); // Gets Card from correct deck.
        //Debug.Log("Sending Card - " + drawnCard);

        RpcReceiveCard(drawnCard.GetComponent<Card>().netId, worksOnDrawingPlayer, choice); // Updates parent of Drawn Card for all clients
    }
    /// <summary>
    /// Sending 4 Doors Cards and 4 Treasures Cards to player 
    /// </summary>
    [Server]
    internal void SendStartingHand()
    {
        StartCoroutine(ServerSendCards(Deck.Doors, 4));
        StartCoroutine(ServerSendCards(Deck.Treasures, 4));
    }
    /// <summary> Sending card to player when syncing objects while loading. </summary>
    /// <param name="card"></param>
    [Server]
    internal void ServerOnLoadReceiveCard( GameObject card )
    {
        NetworkInstanceId cardNetId = card.GetComponent<Card>().GetNetId();
        StartCoroutine(ServerReceiveCard(cardNetId));
    }
    /// <summary> Sending any card to player </summary>
    /// <param name="deck"> Deck from which card is drawn. </param>
    /// <param name="worksOnDrawingPlayer"> If drawn cards are applied to drawing player once they are drawn. </param>
    /// <param name="choice"> If player gets to choose which card to pick. </param>
    [Server]
    internal IEnumerator ServerReceiveCard( NetworkInstanceId cardsNetId, bool worksOnDrawingPlayer = false, bool choice = false )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        RpcReceiveCard(cardsNetId, worksOnDrawingPlayer, choice);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }
    /// <summary> Making correct UI changes when getting new card. </summary>
    /// <param name="deck"> Deck from which card is drawn. </param>
    /// <param name="worksOnDrawingPlayer"> If drawn cards are applied to drawing player once they are drawn. </param>
    /// <param name="choice"> If player gets to choose which card to pick. </param>
    [ClientRpc]
    private void RpcReceiveCard( NetworkInstanceId cardsNetId, bool worksOnDrawingPlayer, bool choice )
    {
        GameObject card = ClientScene.FindLocalObject(cardsNetId);
        card.GetComponent<Card>().ClientSetActiveCardButtons(false);
        card.GetComponent<Draggable>().enabled = true;
        if (!serverGameManager)
            serverGameManager = ServerGameManager.serverGameManager;
        serverGameManager.cardsUsageQueue.Remove(card);
        ClientReceiveCard(card, worksOnDrawingPlayer, choice);
    }
    /// <summary>
    ///  In case of this(PlayerInGame) object being local player(having authority):
    ///     If card needs to be chosen - Innitiate card choosing in choice panel
    ///     If card has effect on player - Run cards effect on this player.
    ///  If this(PlayerInGame) object is not local player - set parent of drawn card to be this' transform.
    /// </summary>
    /// <param name="deck"> Deck from which card is drawn. </param>
    /// <param name="worksOnDrawingPlayer"> If drawn cards are applied to drawing player once they are drawn. </param>
    /// <param name="choice"> If player gets to choose which card to pick. </param>
    [Client]
    private void ClientReceiveCard( GameObject card, bool worksOnDrawingPlayer = false, bool choice = false )
    {
        //Debug.Log("Receiving Card - " + card);

        if (hasAuthority) // if is local player - takes drawn card and puts it in hand
        {
            if (choice) // If cards are selectable => transfer it to choice panel => choose 1 and discard the others
            {
                if (!ChoicePanel.choicePanel.gameObject.activeInHierarchy)
                    ChoicePanel.PrepareToReceiveObjects(ChoicePanelTitle.DrawFirstDoor, choice);

                ChoicePanel.ReceiveObjectToChoose(card);
                return;
            }

            card.transform.SetParent(handContent);  // if client has authority (is owner of drawn card) card is send to handPanel 

            if (worksOnDrawingPlayer)   // If drawn card works on player (ex. first opened doors) it is immediately used against him
                CmdUseCard(card.GetComponent<Card>().netId, this.netId);
        }
        else // if is NOT local player - takes drawn card and puts directly as this objects child
            card.transform.SetParent(this.transform);

        CmdUpdateOwnedCards();
    }
    /// <summary> Activates Proggress button for turn owner only. </summary>
    [ClientRpc]
    internal void RpcTurnOwnerReadiness()
    {
        if (hasAuthority)
            progressButton.ActivateButton();
    }
    /// <summary> Activates Proggress button for all players. </summary>
    [ClientRpc]
    internal void RpcAllPlayersReadiness()
    {
        progressButton.ActivateButton();
    }
    /// <summary> Uses card and target is  local player by default </summary>
    /// <param name="cardNetId"> NetId of card to use. </param>
    internal void UseCardOnLocalPlayer( NetworkInstanceId cardNetId )
    {
        CmdUseCard(cardNetId, this.netId);
    }
    internal void UseStoredCardOnTarget( NetworkInstanceId targetNetId )
    {
        Card storedCard = storedObject.GetComponent<Card>();
        CmdUseCard(storedCard.GetNetId(), targetNetId);
    }
    /// <summary> Uses card on set target. </summary>
    /// <param name="cardNetId"> NetId of card to use. </param>
    /// <param name="targetNetId"> NetId of cards terget. </param>
    internal void UseCard( NetworkInstanceId cardNetId, NetworkInstanceId targetNetId )
    {
        CmdUseCard(cardNetId, targetNetId);
    }

    internal void PutCardOnTable( NetworkInstanceId cardNetId )
    {
        CmdPutCardOnTable(cardNetId);
    }
    [Command]
    void CmdPutCardOnTable( NetworkInstanceId cardNetId )
    {
        Card card = ClientScene.FindLocalObject(cardNetId).GetComponent<Card>();
        card.ServerPutCardOnTable();
    }

    internal void ChoosePlayerToHelp( GameObject chosenObject )
    {
        CmdChoosePlayerToHelp(chosenObject.GetComponent<PlayerInGame>().netId);
    }
    
    internal void ChooseFirstDoors( GameObject chosenDoors )
    {
        progressButton.ActivateButton();

        NetworkInstanceId doorsNetId = chosenDoors.GetComponent<Card>().GetNetId();
        CmdChooseFirstDoors(doorsNetId);
    }

    [Command]
    internal void CmdChooseFirstDoors( NetworkInstanceId doorsNetId )
    {
        Card card = ClientScene.FindLocalObject(doorsNetId).GetComponent<Card>();
        card.ServerUseCardImmediately(this.netId); 
    }

    [Command]
    void CmdUseCard( NetworkInstanceId cardNetId, NetworkInstanceId targetNetId )
    {
        Card card = ClientScene.FindLocalObject(cardNetId).GetComponent<Card>();
        //Debug.Log("CmdUseCard: Card to use: " + card.GetComponent<Card>().cardValues.name);
        StartCoroutine(card.ServerStartAwaitUseConfirmation(targetNetId, this.netId));
    }

    internal void ConfirmCardUsage( bool confirm )
    {
        CmdConfirmCardUsage(confirm);
    }
    [Command]
    void CmdConfirmCardUsage( bool confirm )
    {
        serverGameManager.cardsUsageQueue[0].GetComponent<Card>().ServerConfirmCardUsage(confirm, gameObject);
    }

    internal void InterruptCardUsage()
    {
        CmdInterruptCardUsage();
    }
    [Command]
    void CmdInterruptCardUsage()
    {
        serverGameManager.cardsUsageQueue[0].GetComponent<Card>().ServerInterruptCardUsage();
    }

    internal IEnumerator DiscardCard( List<GameObject> cardsToDiscard )
    {
        foreach (var card in cardsToDiscard)
        {
            CmdDiscardCard(card.GetComponent<Card>().netId);
            yield return new WaitForEndOfFrame();
        }
    }

    [Command]
    internal void CmdDiscardCard( NetworkInstanceId cardNetId )
    {
        StartCoroutine(ServerDiscardCard(cardNetId));
    }

    [Server]
    internal static void OnLoadDiscardCard( GameObject card )
    {
        NetworkInstanceId cardNetId = card.GetComponent<Card>().GetNetId();
        localPlayerInGame.StartCoroutine(ServerDiscardCard(cardNetId));
    }

    [Server]
    IEnumerator ServerDiscardCards( List<GameObject> cardsToDiscard )
    {
        foreach (var card in cardsToDiscard)
        {
            if (CustomNetworkManager.isServerBusy)
                yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
            CustomNetworkManager.isServerBusy = true;

            //Debug.Log("RpcDiscardCardsCoroutine!" + card);
            RpcDiscardCard(card.GetComponent<Card>().netId);

            yield return new WaitForEndOfFrame();
            CustomNetworkManager.isServerBusy = false;
        }
    }

    [Server]
    internal static IEnumerator ServerDiscardCard( NetworkInstanceId cardToDiscardNetId )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        //Debug.Log("RpcDiscardCardCoroutine!" + cardToDiscardNetId);
        localPlayerInGame.RpcDiscardCard(cardToDiscardNetId);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }
    /// <summary>
    /// Moving card to its coresponding discarded deck and updating UI accordingly.
    /// </summary>
    /// <param name="cardNetId"> Discarded card NetworkInstanceId </param>
    [ClientRpc]
    internal void RpcDiscardCard( NetworkInstanceId cardNetId )
    {
        GameObject card = ClientScene.FindLocalObject(cardNetId);
        Card cardScript = card.GetComponent<Card>();
        Debug.Log("Rpc discarding card : " + card);

        cardScript.ClientSetActiveCardButtons(false);
        serverGameManager.cardsUsageQueue.Remove(card);
        card.GetComponent<Draggable>().enabled = true;

        switch (cardScript.cardValues.deck)
        {
            case Deck.Doors:
                cardScript.deck = Deck.DiscardedDoors;
                ServerGameManager.serverGameManager.ServerDecksManager.DiscardedDoorsDeck.GetComponent<DrawCardZone>().ReceiveCard(card.transform);
                break;
            case Deck.Treasures:
                cardScript.deck = Deck.DiscardedTreasures;
                ServerGameManager.serverGameManager.ServerDecksManager.DiscardedTreasuresDeck.GetComponent<DrawCardZone>().ReceiveCard(card.transform);
                break;
            case Deck.HelpingHand:
                break;
            case Deck.Spells:
                break;
            case Deck.DiscardedDoors:
                break;
            case Deck.DiscardedSpells:
                break;
            case Deck.DiscardedTreasures:
                break;
            case Deck.DiscardedHelpingHand:
                break;
            default:
                break;
        }

        StartCoroutine(ClientUpdateOwnedCardsUI());
    }

    [Server]
    internal void FightWon( int treasuresToDrawCount, short levelsGained )
    {
        StartCoroutine(RequestCardsDrawCoroutine(Deck.Treasures, treasuresToDrawCount));
        level += levelsGained;
    }

    /// <summary> Discarding all cards from table. </summary>
    [Server]
    internal void EndFight()
    {
        Debug.Log("ClearTable!");
        List<GameObject> tableGOs = new List<GameObject>();

        foreach (var card in TableDropZone.tableDropZone.transform.GetComponentsInChildren<Card>())
        {
            tableGOs.Add(card.gameObject);
        }
        StartCoroutine(ServerEndFight());
        StartCoroutine(ServerDiscardCards(tableGOs)); // Discarding cards from table
    }

    [Server]
    IEnumerator ServerEndFight()
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        RpcEndFight();

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }
    /// <summary> Disabling fighting UI elements. </summary>
    [ClientRpc]
    void RpcEndFight()
    {
        HelpButton.DeactivateButton();
        ServerGameManager.serverGameManager.fightingMonsters.Clear();
        ServerGameManager.serverGameManager.offeringHelpPlayers.Clear();
        LevelCounter.Deactivate();
    }
    [Server]
    IEnumerator ServerUpdateLevelUI()
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);

        CustomNetworkManager.isServerBusy = true;

        serverGameManager.ServerUpdateFightingPlayersLevel();

        yield return new WaitForEndOfFrame();

        RpcUpdateLevelUI();

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }
    [ClientRpc]
    void RpcUpdateLevelUI()
    {
        StartCoroutine(ClientUpdateLevelUI());
    }
    [Client]
    IEnumerator ClientUpdateLevelUI()
    {
        yield return new WaitUntil(() => opponentStatsUI != null || playerStatsUI != null);
        if (opponentStatsUI != null)
            opponentStatsUI.GetComponent<OpponentInPanel>().UpdateLevel(level);
        else
            playerStatsUI.GetComponent<OpponentInPanel>().UpdateLevel(level);
        // Debug.Log(" fightInProggress - " + serverGameManager.fightInProggres);

        if (serverGameManager.fightInProggres)
            LevelCounter.UpdateLevels();
    }

    [Command]
    void CmdUpdateOwnedCards()
    {
        StartCoroutine(ServerUpdateOwnedCards());
    }
    [Server]
    internal IEnumerator ServerUpdateOwnedCards()
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        RpcUpdateOwnedCards();

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }
    [ClientRpc]
    void RpcUpdateOwnedCards()
    {
        StartCoroutine(ClientUpdateOwnedCardsUI());
    }
    /// <summary>
    /// Checking if this object has authority and then counting and updating number of owned cards accordingly
    /// </summary>
    [Client]
    internal IEnumerator ClientUpdateOwnedCardsUI()
    {
        if (hasAuthority)
            playerStatsUI.GetComponent<OpponentInPanel>().UpdateOwnedCards(handContent.childCount, ownedCardsLimit);
        else
        {
            short ownedCards = 0;

            for (int i = 0; i < transform.childCount; i++)
                if (transform.GetChild(i).gameObject.active) ownedCards++;

            yield return new WaitUntil(() => opponentStatsUI != null);
            opponentStatsUI.GetComponent<OpponentInPanel>().UpdateOwnedCards(ownedCards, ownedCardsLimit);
        }
    }
    /// <summary>
    /// Initiating Choice Panel for choosing player to help in a fight.
    /// </summary>
    internal void RequestHelp()
    {
        if (serverGameManager.offeringHelpPlayers.Count > 0)
        {
            ChoicePanel.PrepareToReceiveObjects(ChoicePanelTitle.ChoosePlayer);
            ChoicePanel.ReceivePlayersToChoose(serverGameManager.offeringHelpPlayers.ToArray());
        }
    }

    internal void OfferHelp()
    {
        CmdOfferHelp();
    }
    [Command]
    void CmdOfferHelp()
    {
        StartCoroutine(ServerOfferHelp());
    }
    [Server]
    IEnumerator ServerOfferHelp()
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        RpcOfferHelp();

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }
    [ClientRpc]
    void RpcOfferHelp()
    {
        // Preventing 1 player to offer help more than once.
        if (serverGameManager.offeringHelpPlayers.Contains(this))
            return;
        // Adding player who offered help to their list.
        serverGameManager.offeringHelpPlayers.Add(this);

        // Updating RequestHelp button text for fighting player
        if (localPlayerInGame.netId == serverGameManager.fightingPlayerNetId)
            HelpButton.UpdateHelpers(true);
    }

    internal void CancelHelp()
    {
        CmdCancelHelp();
    }
    [Command]
    void CmdCancelHelp()
    {
        StartCoroutine(ServerCancelHelp());
    }
    [Server]
    IEnumerator ServerCancelHelp()
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        RpcCancelHelp();

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }
    /// <summary>
    /// Cancelling previously offered help and updating HelpButton text for fighting player
    /// </summary>
    [ClientRpc]
    void RpcCancelHelp()
    {
        if (serverGameManager.offeringHelpPlayers.Contains(this))
            serverGameManager.offeringHelpPlayers.Remove(this);

        if (localPlayerInGame.netId == serverGameManager.fightingPlayerNetId)
            HelpButton.UpdateHelpers(false);
    }

    void ChoosePlayerToHelp( PlayerInGame helpingPlayer )
    {
        CmdChoosePlayerToHelp(helpingPlayer.netId);
    }
    [Command]
    void CmdChoosePlayerToHelp( NetworkInstanceId helpingPlayerNetId )
    {
        StartCoroutine(ServerChoosePlayerToHelp(helpingPlayerNetId));
    }
    /// <summary>
    /// Adding helping players level to fighting players level for a fight.
    /// </summary>
    [Server]
    IEnumerator ServerChoosePlayerToHelp( NetworkInstanceId helpingPlayerNetId )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        serverGameManager.helpingPlayerNetId = helpingPlayerNetId;
        serverGameManager.fightingPlayersLevel += ClientScene.FindLocalObject(helpingPlayerNetId).GetComponent<PlayerInGame>().level;

        RpcChoosePlayerToHelp(helpingPlayerNetId);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }
    /// <summary>
    /// Updating UI upon helping.
    /// </summary>
    [ClientRpc]
    void RpcChoosePlayerToHelp( NetworkInstanceId helpingPlayerNetId )
    {
        HelpButton.DeactivateButton();
        serverGameManager.offeringHelpPlayers.Clear();      // Clearing players which offered help 

        PlayerInGame fightingPlayer = ClientScene.FindLocalObject(serverGameManager.fightingPlayerNetId).GetComponent<PlayerInGame>();
        PlayerInGame helpingPlayer = ClientScene.FindLocalObject(helpingPlayerNetId).GetComponent<PlayerInGame>();
        InfoPanel.Alert(helpingPlayer.NickName + " helps " + fightingPlayer.NickName + " in battle!");
    }

    internal void ChoosePlayerToTradeWith( PlayerInGame[] playersFreeToTrade )
    {
        ChoicePanel.PrepareToReceiveObjects(ChoicePanelTitle.ChoosePlayerToTrade);
        ChoicePanel.ReceivePlayersToChoose(playersFreeToTrade);
    }
    internal void RequestTrade( PlayerInGame requestedPlayer )
    {
        CmdRequestTrade(requestedPlayer.netId);
    }
    [Command]
    void CmdRequestTrade( NetworkInstanceId requestedPlayerNetId )
    {
        GameObject requestedPlayer = ClientScene.FindLocalObject(requestedPlayerNetId);
        StartCoroutine(requestedPlayer.GetComponent<PlayerInGame>().ServerRequestTrade(this.netId));
    }
    [Server]
    IEnumerator ServerRequestTrade( NetworkInstanceId requestedPlayerNetId )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        RpcReveiveTradeRequest(this.netId);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }
    /// <summary>
    /// Player asked for trade gets trade notification.
    /// </summary>
    [ClientRpc]
    void RpcReveiveTradeRequest( NetworkInstanceId requestingPlayerNetId )
    {
        if (hasAuthority)
        {
            PlayerInGame requestingPIG = ClientScene.FindLocalObject(requestingPlayerNetId).GetComponent<PlayerInGame>(); // PIG - PlayerInGame script
            InfoPanel.ReceiveTradeRequestInfo(requestingPIG);
        }
    }
    /// <summary>
    /// Called on asked for trade player when player has accepted trade.
    /// Opening trade panel and sending information that trade request has been accepted.
    /// </summary>
    /// <param name="requestingTradePIG"></param>
    internal void AcceptTradeRequest( PlayerInGame requestingTradePIG )
    {
        TradePanel.PrepareForTrade(requestingTradePIG);
        CmdAcceptTradeRequest(requestingTradePIG.netId);
    }
    /// <summary> 
    /// This is called on asked for trade player object on server.
    /// Finding asking player and telling them that asked player has accepted trade.
    /// </summary>
    [Command]
    void CmdAcceptTradeRequest( NetworkInstanceId requestingTradePIGNetId )
    {
        StartCoroutine(
            ClientScene.FindLocalObject(requestingTradePIGNetId)
            .GetComponent<PlayerInGame>().ServerAcceptTradeRequest(this.netId)
            );
    }
    [Server]
    IEnumerator ServerAcceptTradeRequest( NetworkInstanceId requestingTradePIGNetId )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        RpcReceiveTradeAcceptance(requestingTradePIGNetId);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }
    /// <summary>
    /// Asking player has received info about trade acceptance.
    /// Opens Trade Panel and begins trading.
    /// </summary>
    /// <param name="requestedTradePIGNetId"></param>
    [ClientRpc]
    void RpcReceiveTradeAcceptance( NetworkInstanceId requestedTradePIGNetId )
    {
        if (hasAuthority)
        {
            PlayerInGame requestedTradePIG = ClientScene.FindLocalObject(requestedTradePIGNetId).GetComponent<PlayerInGame>();
            TradePanel.PrepareForTrade(requestedTradePIG);
        }
    }

    internal void DeclineTradeRequest( PlayerInGame requestingTradePIG )
    {
        CmdDeclineTradeRequest(requestingTradePIG.netId);
    }
    [Command]
    void CmdDeclineTradeRequest( NetworkInstanceId requestingTradePIGNetId )
    {
        PlayerInGame requestingTradePIG = ClientScene.FindLocalObject(requestingTradePIGNetId).GetComponent<PlayerInGame>();
        StartCoroutine(requestingTradePIG.ServerDeclineTradeRequest(this.netId));
    }
    [Server]
    IEnumerator ServerDeclineTradeRequest( NetworkInstanceId requestedTradePIGNetId )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        RpcReceiveTradeRequestDenial(requestedTradePIGNetId);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }
    [ClientRpc]
    void RpcReceiveTradeRequestDenial( NetworkInstanceId requestedTradePIGNetId )
    {
        PlayerInGame requestedTradePIG = ClientScene.FindLocalObject(requestedTradePIGNetId).GetComponent<PlayerInGame>();
        if (hasAuthority)
            InfoPanel.ReceiveTradeReqDenial(requestedTradePIG);
    }
    /// <summary>
    /// Called when placing a card in trade panel.
    /// </summary>
    internal void SendTradingCard( GameObject tradingCard)
    {
        Card cardScript = tradingCard.GetComponent<Card>();
        if (!cardScript)
            return;

        // To prevent accidental trade accepting with wrong cards in panels trade acceptance is reset.
        TradePanel.ResetAcceptance();
        CmdSendTradingCard(cardScript.netId, TradePanel.playerWeTradeWith.netId);
    }
    /// <summary>
    /// Finding player we trade with and calling receive method.
    /// </summary>
    /// <param name="tradingCardNetId"> NetId of traded card. </param>
    /// <param name="playerWeTradeWithNetId"> Player who receives trading card </param>
    [Command]
    void CmdSendTradingCard( NetworkInstanceId tradingCardNetId, NetworkInstanceId playerWeTradeWithNetId )
    {
        Debug.Log("CmdReceiveTradingCard()");
        PlayerInGame playerReceivingCard = ClientScene.FindLocalObject(playerWeTradeWithNetId).GetComponent<PlayerInGame>();
        StartCoroutine(playerReceivingCard.ServerReceiveTradingCard(tradingCardNetId));
    }
    /// <summary> Calling Receive card for card receiving clients player object. </summary>
    /// <param name="tradingCardNetId"> NetId of traded card. </param>
    [Server]
    IEnumerator ServerReceiveTradingCard( NetworkInstanceId tradingCardNetId )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        RpcReceiveTradingCard(tradingCardNetId);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }
    /// <summary> Receiving traded card on client. </summary>
    /// <param name="tradingCardNetId"> NetId of traded card. </param>
    [ClientRpc]
    void RpcReceiveTradingCard( NetworkInstanceId tradingCardNetId )
    {
        Debug.Log("RpcReceiveTradingCard()");
        if (hasAuthority)
        {
            GameObject opponentsCard = ClientScene.FindLocalObject(tradingCardNetId);
            TradePanel.ReceiveOpponentsCard(opponentsCard);
        }
    }
    /// <summary> Called when removing card from Trade Panel, on removing client. </summary>
    internal void RemoveTradingCard( GameObject removedCard )
    {
        TradePanel.ResetAcceptance();

        Card cardScript = removedCard.GetComponent<Card>();
        CmdRemoveTradingCard(cardScript.netId);
    }
    /// <summary> Informing server that card has been removed. </summary>
    [Command]
    void CmdRemoveTradingCard( NetworkInstanceId removedCardNetId )
    {
        Debug.Log("CmdRemoveTradingCard()");
        StartCoroutine(ServerRemoveTradingCard(removedCardNetId));
    }
    /// <summary> Calling removing traded card for other client. </summary>
    [Server]
    IEnumerator ServerRemoveTradingCard( NetworkInstanceId removedCardNetId )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        RpcRemoveTradingCard(removedCardNetId);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }
    /// <summary> Removing card from Trade Panel on other client. </summary>
    [ClientRpc]
    void RpcRemoveTradingCard( NetworkInstanceId removedCardNetId )
    {
        if (!hasAuthority)
        {
            GameObject opponentsCard = ClientScene.FindLocalObject(removedCardNetId);
            Debug.Log("RpcRemoveTradingCard(): opponentsCard - " + opponentsCard);
            TradePanel.ResetAcceptance();
            ClientReceiveCard(opponentsCard);
        }
    }
    /// <summary> Called when Accept Trade Button is pressed. </summary>
    internal void AcceptTrade( PlayerInGame playerWeTradeWith )
    {
        CmdAcceptTrade(playerWeTradeWith.netId);
    }
    [Command]
    void CmdAcceptTrade( NetworkInstanceId playerWeTradeWithNetId )
    {
        PlayerInGame playerWeTradeWith = ClientScene.FindLocalObject(playerWeTradeWithNetId).GetComponent<PlayerInGame>();
        StartCoroutine(playerWeTradeWith.ServerAcceptTrade());
    }
    [Server]
    IEnumerator ServerAcceptTrade()
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        RpcAcceptTrade();

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }
    [ClientRpc]
    void RpcAcceptTrade()
    {
        if (hasAuthority)
        {
            TradePanel.TradeAcceptance();
        }
    }
    /// <summary> Called when cancel trade button is pressed. </summary>
    internal void CancelTrade( PlayerInGame playerWeTradeWith )
    {
        CmdCancelTrade(playerWeTradeWith.netId);
    }
    [Command]
    void CmdCancelTrade( NetworkInstanceId playerWeTradeWithNetId )
    {
        StartCoroutine(ServerCancelTrade(playerWeTradeWithNetId)); // Finalizing trade for first player
    }
    [Server]
    IEnumerator ServerCancelTrade( NetworkInstanceId playerWeTradeWithNetId )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        RpcCancelTrade(playerWeTradeWithNetId);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }
    [ClientRpc]
    void RpcCancelTrade( NetworkInstanceId playerWeTradeWithNetId )
    {
        ClientExecuteCancelTrade();

        PlayerInGame playerWeTradeWith = ClientScene.FindLocalObject(playerWeTradeWithNetId).GetComponent<PlayerInGame>();
        playerWeTradeWith.ClientExecuteCancelTrade(); // Finalizing trade for second player
    }
    /// <summary> Return cards from panels to cards owners and deactivate Trade Panel. </summary>
    [Client]
    void ClientExecuteCancelTrade()
    {
        List<GameObject> cardsToReceive = new List<GameObject>();

        if (hasAuthority)
            foreach (Transform card in TradePanel.playersCardsPanel)
                cardsToReceive.Add(card.gameObject);
        else
            foreach (Transform card in TradePanel.opponentsCardsPanel)
                cardsToReceive.Add(card.gameObject);

        foreach (GameObject card in cardsToReceive)
            ClientReceiveCard(card);

        TradePanel.Deactivate();
    }
    /// <summary> Called when both trading players have accepted trade. </summary>
    internal void FinalizeTrade( PlayerInGame playerWeTradeWith )
    {
        CmdFinalizeTrade(playerWeTradeWith.netId);
    }
    [Command]
    void CmdFinalizeTrade( NetworkInstanceId playerWeTradeWithNetId )
    {
        Debug.Log("CmdFinalizeTrade()");
        StartCoroutine(ServerFinalizeTrade(playerWeTradeWithNetId)); // Finalizing trade for first player
    }
    [Server]
    IEnumerator ServerFinalizeTrade( NetworkInstanceId playerWeTradeWithNetId )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        RpcFinalizeTrade(playerWeTradeWithNetId);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }
    /// <summary> Locally calling trade finalization for both trading players. </summary>
    /// <param name="playerWeTradeWithNetId"></param>
    [ClientRpc]
    void RpcFinalizeTrade( NetworkInstanceId playerWeTradeWithNetId )
    {
        Debug.Log("RpcFinalizeTrade()");
        ClientFinalizeTrade(); // Finalizing trade for first player

        PlayerInGame playerWeTradeWith = ClientScene.FindLocalObject(playerWeTradeWithNetId).GetComponent<PlayerInGame>();
        playerWeTradeWith.ClientFinalizeTrade(); // Finalizing trade for second player
    }
    /// <summary> Moving cards from Trade Panel to their new owners. </summary>
    [Client]
    void ClientFinalizeTrade()
    {
        Debug.Log("ExecuteFinalizeTrade()");
        List<GameObject> cardsToReceive = new List<GameObject>();

        if (hasAuthority)
            foreach (Transform card in TradePanel.opponentsCardsPanel)
                cardsToReceive.Add(card.gameObject);
        else
            foreach (Transform card in TradePanel.playersCardsPanel)
                cardsToReceive.Add(card.gameObject);

        foreach (GameObject card in cardsToReceive)
            ClientReceiveCard(card);

        TradePanel.Deactivate();
    }
    /// <summary>
    /// End turn if player can carry cards they're hold to the next round.
    /// If not Alert them.
    /// </summary>
    internal void EndTurn()
    {
        if (ownedCardsLimit >= handContent.GetComponentsInChildren<Card>().Length)
        {
            progressButton.DeactivateButton();
            CmdEndTurn();
        }
        else
        {
            //Debug.Log("You have too many cards on hand! Discard some!");
            InfoPanel.Alert("You have too many cards on hand!\n Discard some!");
        }
    }

    [Command]
    void CmdEndTurn()
    {
        serverGameManager.ServerEndTurn();
    }
    [Server]
    internal IEnumerator ServerPersonalAlertCoroutine( string text )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        RpcPersonalAlert(text);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }
    /// <summary> Executing alert for 1 player only. </summary>
    [ClientRpc]
    void RpcPersonalAlert( string text )
    {
        if (hasAuthority)
            InfoPanel.Alert(text);
    }
    /// <summary> Called when saving game. </summary>
    /// <returns> Returns certain player stats. </returns>
    internal PlayerSaveData GetPlayerData()
    {
        PlayerSaveData playerData = new PlayerSaveData();

        playerData.hasTurn = hasTurn;
        playerData.isAlive = isAlive;
        playerData.level = level;
        playerData.nickName = NickName;

        List<string> cardsInHand = new List<string>();
        List<string> equippedItems = new List<string>();

        for (int i = 0; i < handContent.childCount; i++)
            cardsInHand.Add(handContent.GetChild(i).GetComponent<Card>().GetCardData());

        for (int i = 0; i < this.equippedItems.Count; i++)
            equippedItems.Add(this.equippedItems[i].GetComponent<Card>().GetCardData());

        playerData.cardsInHand = cardsInHand;
        playerData.equippedItems = equippedItems;

        return playerData;
    }
    /// <summary> Resyncing UI and hierarchy objects on reconnected player. </summary>
    [Server]
    internal void ServerReconnect()
    {
        if (!hasAuthority)
            handContent = transform;

        Debug.Log("ServerReconnect(): handContent - " + handContent);
        for (int i = 0; i < handContent.childCount; i++)
        {
            GameObject card = handContent.GetChild(i).gameObject;
            ServerOnLoadReceiveCard(card);
        }

        foreach (var item in equippedItems)
            StartCoroutine(ServerOnReconnectEquip(item));

        StartCoroutine(ServerUpdateLevelUI());
    }
}
