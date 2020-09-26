#pragma warning disable CS0618 // Typ lub składowa jest przestarzała

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerInGame : NetworkBehaviour
{
    internal ServerGameManager serverGameManager; // Reference to ServerGameManager script for easier use here.
    internal static PlayerInGame localPlayerInGame; // Static reference to PlayerInGame(this) script, it gets set by the object that is meant to be local player.

    CustomNetworkManager CustomNetworkManager;

    [SyncVar] internal string NickName; // Nick name of player got from options in title screen.
    internal Sprite Avatar; // Not Implemented Yet

    //      Statystyki      //
    [SyncVar] [SerializeField] private short level = 1; // Level of a player
    
    public short Level
    {
        get => level;
        set
        {
            level = value;
            StartCoroutine(ServerUpdateLevelUI());  // Updating Level in UI on change
        }
    }

    [SyncVar] private short numberOfHands;   // Describes how many weapons can player use at once, Not Implemented Yet.
    [SyncVar] private short bigItemsLimit;   // Describes how many "Big" items can player use at once, Not Implemented Yet.
    [SyncVar] private short ownedCardsLimit; // Limits maximum number of cards player can own at end of their turn.

    [SyncVar] private short luck;       // Modifies random outcomes of cards effects, Not Implemented Yet
    [SyncVar] internal bool canPlayerDrawHelpHand = false;  // Not Implemented Yet
    [SyncVar] internal bool canPlayerDrawSpells = false;    // Not Implemented Yet

    [SyncVar] internal bool isPlayerAlive;                           // Player cannot perform any actions until ressurected, Not Implemented Yet
    [SyncVar] [SerializeField] internal bool hasTurn = false; // Certain actions are only available when player has turn.

    //      UI      //
    [SerializeField] internal GameObject opponentInPanel;   // Reference to opponentInPanel Prefab
    [SerializeField] internal GameObject playerCanvas;      // Reference to playerCanvas Prefab

    internal ProgressButton ProgressButton; // Reference to Progress Button object. Enables managing of if player has to report readiness or not.
    internal HelpButton HelpButton;         // Reference to Help Button object. Enables managing of "helping in combat" mechanic.

    internal Transform handContent;  // Reference to players "Hand" Transform that stores players owned cards.

    internal Transform table;           // Reference to "Table" Transform that stores cards being currently in use.
    private Transform opponentsPanel;   // Reference to panels Transform that stores objects (OpponentInPanel) which show information about enemies
    internal Transform equipment;        // Reference to Transform that stores slots for eqipment.

    internal ChoicePanel choicePanel;    // Reference to "ChoicePanel" script that manages choosing cards.
    private TradePanel tradePanel;      // Referance to "TradePanel" script that manages trading cards with other players.
    internal LevelCounter levelCounter;

    //      Equipment       //
    [SerializeField] internal List<GameObject> equippedItems = new List<GameObject>();
    [SerializeField] List<GameObject> activeBuffs = new List<GameObject>(); // Not Implemented Yet

    [SerializeField] internal GameObject storedObject = null; // Reference to store selected card, to choose target for it.

    void Start()
    {
        serverGameManager = ServerGameManager.serverGameManager;
        CustomNetworkManager = CustomNetworkManager.customNetworkManager;
        table = serverGameManager.ServerDecksManager.Table;

        if (hasAuthority) // Check if player "Owns" this object, this is set in "PlayerManager" script.
        {
            localPlayerInGame = this;
            CmdStart(GlobalVariables.NickName);

            playerCanvas = Instantiate(playerCanvas); // Gets prefab stored in "playerCanvas" variable -> creates it as GameObject -> and stores reference to it in same variable

            //      Setting references for UI Objects      //

            opponentInPanel = playerCanvas.transform.Find("PlayerInPanel").gameObject;
            opponentInPanel.GetComponent<OpponentInPanel>().Initialize(this);

            equipment = opponentInPanel.transform.Find("Eq");
            handContent = playerCanvas.transform.Find("HandPanel").Find("Content");

            ProgressButton = playerCanvas.transform.Find("ProgressButton").GetComponent<ProgressButton>();
            HelpButton = playerCanvas.transform.Find("HelpButton").GetComponent<HelpButton>();

            choicePanel = playerCanvas.transform.Find("ChoicePanel").GetComponent<ChoicePanel>();
            choicePanel.Initialize();

            tradePanel = playerCanvas.transform.Find("TradePanel").GetComponent<TradePanel>();
            tradePanel.Initialize();

            levelCounter = playerCanvas.transform.Find("LevelCounter").GetComponent<LevelCounter>();
            levelCounter.Initialize();

            ///////////////////////////////////////////////////
        }
        else // If player doesn't own this object
        {
            Debug.Log("localPlayerInGame - " + localPlayerInGame);

            if (localPlayerInGame)      // If local player static variable has been set by local players object
                                        // Initializing enemy panel ui object for local player
            {
                opponentsPanel = localPlayerInGame.playerCanvas.transform.Find("OpponentsPanel");
                opponentInPanel = Instantiate(opponentInPanel);
                opponentInPanel.transform.SetParent(opponentsPanel);
                opponentInPanel.GetComponent<OpponentInPanel>().Initialize(this);
                equipment = opponentInPanel.transform.Find("EnemyEq");
            }
            else // If localPlayerInGame hasn't been set yet, wait for it
                StartCoroutine(InitializeEnemiesPanel());
        }
    }

    IEnumerator InitializeEnemiesPanel()
    {
        // Debug.Log("InCoroutine - InitializeEnemiesPanel");
        while (!localPlayerInGame || !localPlayerInGame.playerCanvas) // Waiting until needed references are set by the local player
        {
            Debug.Log("Waiting for localPlayerInGame");
            yield return new WaitForSeconds(0.25f);
        }

        opponentsPanel = localPlayerInGame.playerCanvas.transform.Find("OpponentsPanel");
        opponentInPanel = Instantiate(opponentInPanel);
        opponentInPanel.transform.SetParent(opponentsPanel);
        opponentInPanel.GetComponent<OpponentInPanel>().Initialize(this);
        equipment = opponentInPanel.transform.Find("EnemyEq");
    }

    [Command]
    void CmdStart( string NickName ) // Initializes variables (for this object) on the server that are then set on clients
    {
        numberOfHands = 2;
        bigItemsLimit = 1;
        ownedCardsLimit = 5;
        luck = 0;
        isPlayerAlive = true;
        StartCoroutine(serverGameManager.ReportPresence(this.netId));
        this.NickName = NickName;
    }

    [Command]
    internal void CmdReadyPlayersUp() // Calling readiness on server
    {
        serverGameManager.ReadyPlayersUp();
    }

    [Server]
    internal IEnumerator ServerEquip( NetworkInstanceId cardNetId )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        GameObject card = ClientScene.FindLocalObject(cardNetId); // Finding card locally via its "NetworkInstanceId"

        Level += card.GetComponent<EquipmentCard>().cardValues.level;  // Modifying players level according to cards level variable
        RpcEquip(cardNetId); // Calling equiping card on all clients
    }

    internal void LocalEquip( GameObject card )
    {
        if (hasAuthority)
            Destroy(card.GetComponent<Draggable>().Placeholder);

        //Debug.Log("Equipping " + equipmentCard.cardValues.name + " in the " + equipmentCard.eqPart + " slot");
        equippedItems.Add(card);                                            // player.equippedItems.Add(this.gameObject);                                                   

        EquipmentCard eqCardScript = card.GetComponent<EquipmentCard>();    // Setting item in item slot UI for players that don't own this item
        EqItemSlot eqSlot = equipment.GetChild((int)(eqCardScript.cardValues as EquipmentValue).eqPart).GetComponentInChildren<EqItemSlot>();

        eqSlot.ReceiveEq(card);
        LocalUpdateOwnedCards();
        LocalUpdateLevelUI();
    }


    [ClientRpc]
    void RpcEquip( NetworkInstanceId cardNetId ) // Calling equiping card on all clients
    {
        GameObject eqCard = ClientScene.FindLocalObject(cardNetId); // Finding card locally via its "NetworkInstanceId"
        equippedItems.Add(eqCard);                                  // adding card to equipped items List

        EquipmentCard eqCardScript = eqCard.GetComponent<EquipmentCard>(); // Setting item in item slot UI for players that don't own this item
        equipment.GetChild((int)(eqCardScript.cardValues as EquipmentValue).eqPart).GetComponentInChildren<EqItemSlot>().ReceiveEq(eqCard);
    }

    internal void Unequip( GameObject card )
    {
        CmdUnequip(card.GetComponent<Card>().netId);
    }

    [Command]
    internal void CmdUnequip( NetworkInstanceId cardNetId )
    {
        StartCoroutine(ServerUnequip(cardNetId));
    }

    [Server]
    IEnumerator ServerUnequip( NetworkInstanceId cardNetId )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        GameObject card = ClientScene.FindLocalObject(cardNetId);
        Debug.Log("Unequipping - " + card);

        Level -= card.GetComponent<EquipmentCard>().cardValues.level;

        RpcUnequip(cardNetId);
    }

    [ClientRpc]
    internal void RpcUnequip( NetworkInstanceId cardNetId )
    {
        LocalUnequip(cardNetId);
    }

    internal void LocalUnequip( NetworkInstanceId cardNetId )
    {
        GameObject eqCard = ClientScene.FindLocalObject(cardNetId);
        equippedItems.Remove(eqCard);

        EquipmentCard eqCardScript = eqCard.GetComponent<EquipmentCard>();
        EqItemSlot eqSlot = equipment.GetChild((int)(eqCardScript.cardValues as EquipmentValue).eqPart).GetComponentInChildren<EqItemSlot>();

        if (!hasAuthority)
            eqSlot.ReturnEnemyEq();

        LocalUpdateOwnedCards();

        opponentInPanel.GetComponent<OpponentInPanel>().UpdateLevel(level);
    }

    internal void LocalSwitchEq( NetworkInstanceId cardNetId )
    {
        GameObject eqCard = ClientScene.FindLocalObject(cardNetId);
        equippedItems.Remove(eqCard);

        EquipmentCard eqCardScript = eqCard.GetComponent<EquipmentCard>();
        EqItemSlot eqSlot = equipment.GetChild((int)(eqCardScript.cardValues as EquipmentValue).eqPart).GetComponentInChildren<EqItemSlot>();

        if (!hasAuthority)
            eqSlot.ReturnEnemyEq();
        else
            eqSlot.ReturnEq();

        LocalEquip(eqCard);
        LocalUpdateOwnedCards();
    }

    private void FindTable()
    {
        if (!table)
            table = localPlayerInGame.table;
    }


    private IEnumerator RequestCardDrawCoroutine( Deck deck, int quantity, bool worksOnDrawingPlayer = false, bool choice = false )
    {
        for (int i = 0; i < quantity; i++)
        {
            if (CustomNetworkManager.isServerBusy)
                yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
            CustomNetworkManager.isServerBusy = true;
            CmdRequestCardDraw(deck, worksOnDrawingPlayer, choice);
        }
    }

    [Command]
    private void CmdRequestCardDraw( Deck deck, bool worksOnDrawingPlayer, bool choice )
    {
        ServerSendCard(deck, worksOnDrawingPlayer, choice);
    }

    [Server]
    internal IEnumerator ServerSendCardsCoroutine( Deck deck, int quantity, bool worksOnDrawingPlayer = false, bool choice = false )
    {
        //Debug.Log("ServerSendCardsCoroutine");
        for (int i = 0; i < quantity; i++)
        {
            if (CustomNetworkManager.isServerBusy)
                yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);

            CustomNetworkManager.isServerBusy = true;
            ServerSendCard(deck, worksOnDrawingPlayer, choice);
        }
    }

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
                drawCardZone = serverGameManager.ServerDecksManager.DoorsDeck.GetComponent<DrawCardZone>();
                last = serverGameManager.ServerDecksManager.Doors.Count - 1; // Number of cards in deck - 1
                break;
            case Deck.Treasures:
                drawCardZone = serverGameManager.ServerDecksManager.TreasuresDeck.GetComponent<DrawCardZone>();
                last = serverGameManager.ServerDecksManager.Treasures.Count - 1;
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
        Debug.Log("Sending Card - " + drawnCard);

        RpcReceiveCard(drawnCard.GetComponent<Card>().netId, this.netId, worksOnDrawingPlayer, choice); // Updates parent of Drawn Card for all clients

        // RemoveLastCardFromDecksList(deck);
    }

    [Server]
    internal void SendStartingHand() // Sending 4 Doors Cards and 4 Treasures Cards for each players objects
    {
        // Only one network method can be called per frame so we need Coroutines
        StartCoroutine(ServerSendCardsCoroutine(Deck.Doors, 4));
        StartCoroutine(ServerSendCardsCoroutine(Deck.Treasures, 4));
    }

    [Server]
    internal IEnumerator ServerReceiveCard( NetworkInstanceId cardsNetId, NetworkInstanceId newCardParentsId, bool worksOnDrawingPlayer, bool choice )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;
        RpcReceiveCard(cardsNetId, newCardParentsId, worksOnDrawingPlayer, choice);
    }

    [ClientRpc]
    private void RpcReceiveCard( NetworkInstanceId cardsNetId, NetworkInstanceId newCardParentsId, bool worksOnDrawingPlayer, bool choice )
    {
        GameObject drawnCard = ClientScene.FindLocalObject(cardsNetId);
        ReceiveCard(drawnCard, worksOnDrawingPlayer, choice);
    }

    IEnumerator RpcReceiveCardCoroutine( NetworkInstanceId cardsNetId, NetworkInstanceId newCardParentsId, bool worksOnDrawingPlayer, bool choice )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;
        RpcReceiveCard(cardsNetId, newCardParentsId, worksOnDrawingPlayer, choice);
    }

    private void ReceiveCard( GameObject card, bool worksOnDrawingPlayer = false, bool choice = false )
    {
        //Debug.Log("Receiving Card - " + card);

        if (hasAuthority) // if is local player - takes drawn card and puts it in hand
        {
            if (choice) // If cards are selectable => transfer it to choice panel => choose 1 and discard the others
            {
                if (!choicePanel.gameObject.activeInHierarchy)
                    choicePanel.PrepareToReceiveObjects(ChoicePanelTitle.DrawFirstDoor, choice);

                choicePanel.ReceiveObjectToChoose(card);
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

    [Server]
    private void RemoveLastCardFromDecksList( Deck deck )
    {
        switch (deck)
        {
            case Deck.Doors:
                serverGameManager.ServerDecksManager.Doors.RemoveAt(serverGameManager.ServerDecksManager.Doors.Count - 1);
                break;
            case Deck.Treasures:
                serverGameManager.ServerDecksManager.Treasures.RemoveAt(serverGameManager.ServerDecksManager.Treasures.Count - 1);
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
    }

    [Server] // Called on Server
    internal void StartTurn()
    {

    }

    [ClientRpc]
    internal void RpcTurnOwnerReadiness() // Activates readiness checking for turn owner only
    {
        if (hasAuthority)
            ProgressButton.ActivateButton();
    }

    [ClientRpc]
    internal void RpcAllPlayersReadiness() // Activates readiness checking for all players
    {
        localPlayerInGame.ProgressButton.ActivateButton();
    }

    internal void UseCard( NetworkInstanceId cardNetId )
    {
        CmdUseCard(cardNetId, this.netId);
    }

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
        StartCoroutine(ServerPutCardOnTable(cardNetId));
    }

    [Server]
    IEnumerator ServerPutCardOnTable( NetworkInstanceId cardNetId )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;
        RpcPutCardOnTable(cardNetId);
    }

    void RpcPutCardOnTable( NetworkInstanceId cardNetId )
    {
        ClientScene.FindLocalObject(cardNetId).transform.SetParent(table);
    }

    internal void UseCardOnTarget( GameObject chosenObject )
    {
        if (storedObject != null) // Before every selection of cards target, card for which we choose target has to be stored in "storedObject"
        {
            CmdUseCard(storedObject.GetComponent<Card>().netId, chosenObject.GetComponent<NetworkBehaviour>().netId);
            storedObject = null;
        }
        else // Since we don't have anything stored in "storedObject" we don't choose target for card, instead we must choose player to help.
        {
            CmdChoosePlayerToHelp(chosenObject.GetComponent<PlayerInGame>().netId);
        }
    }

    [Command]
    void CmdUseCard( NetworkInstanceId cardNetId, NetworkInstanceId targetNetId )
    {
        Card card = ClientScene.FindLocalObject(cardNetId).GetComponent<Card>();
        // Debug.Log("Cmd found Card to use: " + card.GetComponent<Card>().cardValues.name);
        StartCoroutine(card.EffectOnUse(targetNetId)); // Using Cards Effect on chosen target
    }

    public void EnableTable()
    {
        FindTable();
        table.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    internal void DisableTable()
    {
        table.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    // Odrzucanie kart
    public void RequestDiscardCards( List<GameObject> cardsToDiscard )
    {
        StartCoroutine(ClientDiscardCard(cardsToDiscard));
    }

    IEnumerator ClientDiscardCard( List<GameObject> cardsToDiscard )
    {
        foreach (var card in cardsToDiscard)
        {
            if (CustomNetworkManager.isServerBusy)
                yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
            CustomNetworkManager.isServerBusy = true;
            CmdDiscardCard(card.GetComponent<Card>().netId);
        }
    }

    [Command]
    internal void CmdDiscardCard( NetworkInstanceId cardNetId )
    {
        StartCoroutine(ServerDiscardCard(cardNetId));
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
        }
    }

    [Server]
    internal IEnumerator ServerDiscardCard( NetworkInstanceId cardToDiscardNetId )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;
        //Debug.Log("RpcDiscardCardCoroutine!" + cardToDiscardNetId);
        RpcDiscardCard(cardToDiscardNetId);
    }

    [ClientRpc]
    internal void RpcDiscardCard( NetworkInstanceId cardNetId )
    {
        GameObject card = ClientScene.FindLocalObject(cardNetId);
        Card cardScript = card.GetComponent<Card>();
        Debug.Log("Rpc discarding card : " + card);

        switch (cardScript.cardValues.deck)
        {
            case Deck.Doors:
                cardScript.deck = Deck.DiscardedDoors;
                serverGameManager.ServerDecksManager.DiscardedDoorsDeck.GetComponent<DrawCardZone>().ReceiveCard(card.transform);
                break;
            case Deck.Treasures:
                cardScript.deck = Deck.DiscardedTreasures;
                serverGameManager.ServerDecksManager.DiscardedTreasuresDeck.GetComponent<DrawCardZone>().ReceiveCard(card.transform);
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

        LocalUpdateOwnedCards();
    }

    [Server]
    internal void FightWon( int treasuresToDrawCount, short levelsGained )
    {
        StartCoroutine(RequestCardDrawCoroutine(Deck.Treasures, treasuresToDrawCount));
        level += levelsGained; // TODO: Check Level win condition in CmdSetLevel()
    }

    internal void EndTurn()
    {
        if (ownedCardsLimit >= handContent.GetComponentsInChildren<Card>().Length) // If player has correct number of cards in hand
        {
            ProgressButton.DeactivateButton();
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
        serverGameManager.EndTurn();
    }

    [Server]
    internal void EndFight()
    {
        Debug.Log("ClearTable!");
        List<GameObject> tableGOs = new List<GameObject>();

        foreach (var card in table.GetComponentsInChildren<Card>())
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
    }

    [ClientRpc]
    void RpcEndFight()
    {
        localPlayerInGame.HelpButton.DeactivateButton();
        ServerGameManager.serverGameManager.fightingMonsters.Clear();
        ServerGameManager.serverGameManager.offeringHelpPlayers.Clear();
    }

    IEnumerator ServerUpdateLevelUI()
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);

        CustomNetworkManager.isServerBusy = true;

        serverGameManager.UpdateFightingPlayersLevel();

        yield return new WaitForEndOfFrame();

        RpcUpdateLevelUI();
    }

    [ClientRpc]
    void RpcUpdateLevelUI()
    {
        LocalUpdateLevelUI();
    }

    void LocalUpdateLevelUI()
    {
        opponentInPanel.GetComponent<OpponentInPanel>().UpdateLevel(level);

        Debug.Log(" fightInProggress - " + serverGameManager.fightInProggres);

        if (serverGameManager.fightInProggres)
            localPlayerInGame.levelCounter.UpdateLevels();
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
    }

    [ClientRpc]
    void RpcUpdateOwnedCards()
    {
        LocalUpdateOwnedCards();
    }

    internal void LocalUpdateOwnedCards()
    {
        if (hasAuthority)
            opponentInPanel.GetComponent<OpponentInPanel>().UpdateOwnedCards(handContent.childCount, ownedCardsLimit);
        else
        {
            short ownedCards = 0;

            for (int i = 0; i < transform.childCount; i++)
                if (transform.GetChild(i).gameObject.active) ownedCards++;

            opponentInPanel.GetComponent<OpponentInPanel>().UpdateOwnedCards(ownedCards, ownedCardsLimit);
        }
    }

    internal void RequestHelp()
    {
        if (serverGameManager.offeringHelpPlayers.Count > 0)
        {
            choicePanel.PrepareToReceiveObjects(ChoicePanelTitle.ChoosePlayer);
            choicePanel.ReceivePlayersToChoose(serverGameManager.offeringHelpPlayers.ToArray());
        }
    }

    internal void OfferHelp()
    {
        CmdOfferHelp();
    }

    [Command]
    internal void CmdOfferHelp()
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
    }

    [ClientRpc]
    internal void RpcOfferHelp()
    {
        //if (!localPlayerInGame.helpChoiceButton.gameObject.activeInHierarchy)
        //    localPlayerInGame.helpChoiceButton.gameObject.SetActive(true);

        if (serverGameManager.offeringHelpPlayers.Contains(this))
            return;

        serverGameManager.offeringHelpPlayers.Add(this);

        if (localPlayerInGame.netId == serverGameManager.fightingPlayerNetId)
            localPlayerInGame.HelpButton.UpdateHelpers(true);
    }

    internal void CancelHelp()
    {
        CmdCancelHelp();
    }

    [Command]
    internal void CmdCancelHelp()
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
    }

    [ClientRpc]
    internal void RpcCancelHelp()
    {
        //if (!localPlayerInGame.helpChoiceButton.gameObject.activeInHierarchy)
        //    localPlayerInGame.helpChoiceButton.gameObject.SetActive(true);

        if (serverGameManager.offeringHelpPlayers.Contains(this))
            serverGameManager.offeringHelpPlayers.Remove(this);

        if (localPlayerInGame.netId == serverGameManager.fightingPlayerNetId)
            localPlayerInGame.HelpButton.UpdateHelpers(false);
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

    [Server]
    IEnumerator ServerChoosePlayerToHelp( NetworkInstanceId helpingPlayerNetId )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        serverGameManager.helpingPlayerNetId = helpingPlayerNetId;
        serverGameManager.fightingPlayersLevel += ClientScene.FindLocalObject(helpingPlayerNetId).GetComponent<PlayerInGame>().level;

        RpcChoosePlayerToHelp(helpingPlayerNetId);
    }

    [ClientRpc]
    void RpcChoosePlayerToHelp( NetworkInstanceId helpingPlayerNetId )
    {
        localPlayerInGame.HelpButton.DeactivateButton();
        serverGameManager.offeringHelpPlayers.Clear();      // Clearing players which offered help 

        PlayerInGame fightingPlayer = ClientScene.FindLocalObject(serverGameManager.fightingPlayerNetId).GetComponent<PlayerInGame>();
        PlayerInGame helpingPlayer = ClientScene.FindLocalObject(helpingPlayerNetId).GetComponent<PlayerInGame>();
        InfoPanel.Alert(helpingPlayer.NickName + " helps " + fightingPlayer.NickName + " in battle!");
    }

    internal void ChoosePlayerToTradeWith( PlayerInGame[] playersFreeToTrade )
    {
        choicePanel.PrepareToReceiveObjects(ChoicePanelTitle.ChoosePlayerToTrade);
        choicePanel.ReceivePlayersToChoose(playersFreeToTrade);
    }

    internal void RequestTrade( PlayerInGame requestedPlayer )
    {
        CmdRequestTrade(requestedPlayer.netId);
    }

    [Command]
    internal void CmdRequestTrade( NetworkInstanceId requestedPlayerNetId )
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
    }

    [ClientRpc]
    internal void RpcReveiveTradeRequest( NetworkInstanceId requestingPlayerNetId )
    {
        if (hasAuthority)
        {
            PlayerInGame requestingPIG = ClientScene.FindLocalObject(requestingPlayerNetId).GetComponent<PlayerInGame>(); // PIG - PlayerInGame script
            InfoPanel.ReceiveTradeReqInfo(requestingPIG);
        }
    }

    internal void AcceptTradeRequest( PlayerInGame requestingTradePIG ) // this is called on REQUESTED player object locally
    {
        tradePanel.PrepareForTrade(requestingTradePIG);
        // Open Trade Panel, Can add owned cards to panel or view cards added by enemy.
        CmdAcceptTradeRequest(requestingTradePIG.netId); // Calling trade acceptance on server.
    }

    [Command]
    void CmdAcceptTradeRequest( NetworkInstanceId requestingTradePIGNetId ) // this is called on REQUESTED player object on server
    {
        // In here we tell REQUESTING player that REQUESTED player accepted trade.
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
    }

    [ClientRpc]
    void RpcReceiveTradeAcceptance( NetworkInstanceId requestedTradePIGNetId ) // this is called on REQUESTING player object locally
    {
        if (hasAuthority)
        {
            // Open Trade Panel, Can add owned cards to panel or view cards added by enemy.
            PlayerInGame requestedTradePIG = ClientScene.FindLocalObject(requestedTradePIGNetId).GetComponent<PlayerInGame>();

            tradePanel.PrepareForTrade(requestedTradePIG);
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
        RpcReceiveTradeRequestDeclination(requestedTradePIGNetId);
    }

    [ClientRpc]
    void RpcReceiveTradeRequestDeclination( NetworkInstanceId requestedTradePIGNetId )
    {
        PlayerInGame requestedTradePIG = ClientScene.FindLocalObject(requestedTradePIGNetId).GetComponent<PlayerInGame>();
        if (hasAuthority)
            InfoPanel.ReceiveTradeReqDenial(requestedTradePIG);
    }

    internal void ReceiveTradingCard( GameObject tradingCard, PlayerInGame playerWeTradeWith ) // Called on player object which has authority, becouse [Command] methods may be called only from such objects.
    {
        Card cardScript = tradingCard.GetComponent<Card>();
        if (!cardScript)
            return;

        tradePanel.ResetAcceptance();
        CmdReceiveTradingCard(cardScript.netId, playerWeTradeWith.netId);
    }

    [Command]
    internal void CmdReceiveTradingCard( NetworkInstanceId tradingCardNetId, NetworkInstanceId playerWeTradeWithNetId ) // Player object that receives card calls
    {
        Debug.Log("CmdReceiveTradingCard()");
        PlayerInGame playerReceivingCard = ClientScene.FindLocalObject(playerWeTradeWithNetId).GetComponent<PlayerInGame>();
        StartCoroutine(playerReceivingCard.ServerReceiveTradingCard(tradingCardNetId));
    }

    [Server]
    IEnumerator ServerReceiveTradingCard( NetworkInstanceId tradingCardNetId )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;
        RpcReceiveTradingCard(tradingCardNetId);
    }

    [ClientRpc]
    internal void RpcReceiveTradingCard( NetworkInstanceId tradingCardNetId ) // This is called on player object which receives card
    {
        Debug.Log("RpcReceiveTradingCard()");
        if (hasAuthority)
        {
            GameObject enemysCard = ClientScene.FindLocalObject(tradingCardNetId);
            tradePanel.ReceiveEnemysCard(enemysCard);
        }
    }

    internal void RemoveTradingCard( GameObject tradingCard ) // This is called on player which receives card
    {
        tradePanel.ResetAcceptance();

        Card cardScript = tradingCard.GetComponent<Card>();
        CmdRemoveTradingCard(cardScript.netId);
    }

    [Command]
    void CmdRemoveTradingCard( NetworkInstanceId tradingCardNetId ) // This is called on player which receives card
    {
        Debug.Log("CmdRemoveTradingCard()");
        StartCoroutine(ServerRemoveTradingCard(tradingCardNetId));
    }

    [Server]
    IEnumerator ServerRemoveTradingCard( NetworkInstanceId tradingCardNetId )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;
        RpcRemoveTradingCard(tradingCardNetId);
    }

    [ClientRpc]
    void RpcRemoveTradingCard( NetworkInstanceId tradingCardNetId ) // This is called on player which receives card
    {
        if (!hasAuthority)
        {
            GameObject opponentsCard = ClientScene.FindLocalObject(tradingCardNetId);
            Debug.Log("RpcRemoveTradingCard(): opponentsCard - " + opponentsCard);
            //tradePanel.RemoveEnemysCard(enemysCard);
            TradePanel.tradePanel.ResetAcceptance();
            ReceiveCard(opponentsCard);
        }
    }

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
    }

    [ClientRpc]
    void RpcAcceptTrade()
    {
        if (hasAuthority)
        {
            tradePanel.TradeAcceptance();
        }
    }

    internal void CancelTrade( PlayerInGame playerWeTradeWith )
    {
        CmdCancelTrade(playerWeTradeWith.netId);
    }

    [Command]
    internal void CmdCancelTrade( NetworkInstanceId playerWeTradeWithNetId )
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
    }

    [ClientRpc]
    void RpcCancelTrade( NetworkInstanceId playerWeTradeWithNetId )
    {
        ExecuteCancelTrade();

        PlayerInGame playerWeTradeWith = ClientScene.FindLocalObject(playerWeTradeWithNetId).GetComponent<PlayerInGame>();
        playerWeTradeWith.ExecuteCancelTrade(); // Finalizing trade for second player
    }

    void ExecuteCancelTrade()
    {
        List<GameObject> cardsToReceive = new List<GameObject>();

        if (hasAuthority) // If player object is local Player  || player object matches the on we traded with
        {
            // Receive cards from tradePanels playersCardsPanel
            foreach (Transform card in tradePanel.playersCardsPanel)
            {
                cardsToReceive.Add(card.gameObject);
            }
        }
        else
        {
            foreach (Transform card in TradePanel.tradePanel.opponentsCardsPanel)
            {
                cardsToReceive.Add(card.gameObject);
            }
        }

        foreach (GameObject card in cardsToReceive)
        {
            ReceiveCard(card);
        }

        TradePanel.tradePanel.Rest();
    }

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
    }

    [ClientRpc]
    void RpcFinalizeTrade( NetworkInstanceId playerWeTradeWithNetId )
    {
        Debug.Log("RpcFinalizeTrade()");
        ExecuteFinalizeTrade(); // Finalizing trade for first player

        PlayerInGame playerWeTradeWith = ClientScene.FindLocalObject(playerWeTradeWithNetId).GetComponent<PlayerInGame>();
        playerWeTradeWith.ExecuteFinalizeTrade(); // Finalizing trade for second player
    }

    void ExecuteFinalizeTrade()
    {
        Debug.Log("ExecuteFinalizeTrade()");
        List<GameObject> cardsToReceive = new List<GameObject>();

        if (hasAuthority) // If player object is local Player
        {
            // Receive cards from tradePanels 
            foreach (Transform card in tradePanel.opponentsCardsPanel)
            {
                cardsToReceive.Add(card.gameObject);
            }
        }
        else
        {
            foreach (Transform card in TradePanel.tradePanel.playersCardsPanel)
            {
                cardsToReceive.Add(card.gameObject);
            }
        }

        foreach (GameObject card in cardsToReceive)
        {
            ReceiveCard(card);
        }

        TradePanel.tradePanel.Rest();
    }

    [Server]
    internal IEnumerator ServerPersonalAlertCoroutine( string text )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;
        RpcPersonalAlert(text);
    }

    [ClientRpc]
    void RpcPersonalAlert( string text )
    {
        if (hasAuthority)
            InfoPanel.Alert(text);
    }

    internal string GetEqFrom( EqPart eq )
    {
        if (equipment.GetChild((int)eq).GetComponent<EqItemSlot>().heldItem)
            return equipment.GetChild((int)eq).GetComponent<EqItemSlot>().heldItem.name;
        else return null;
    }
}
