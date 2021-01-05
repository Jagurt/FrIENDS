#pragma warning disable CS0618 // Type is too old
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public enum Deck { Doors, Treasures, HelpingHand, Spells, DiscardedDoors, DiscardedSpells, DiscardedTreasures, DiscardedHelpingHand };
public enum TurnPhase { Beginning, Search };
enum GamePhase { PreStart, InProgress };
public enum EqPart { Head, Chest, Hands, Legs, Feet, Ring, Weapon1, Weapon2 };

/// <summary> Game Manager class containing general game variables and methods </summary>
public class GameManager : NetworkBehaviour
{
    //      Stored References       //
    internal static GameManager singleton;
    CustomNetManager CustomNetworkManager;
    private DecksManager serverDecksManager;
    internal DecksManager ServerDecksManager { get => serverDecksManager; }

    // Variables defining phase of game and turn.
    [SerializeField] GamePhase GamePhase = GamePhase.PreStart;
    /// <summary>
    /// [SyncVar] - decorator for variables that have to be synchronized on all clients by server.
    /// Those variables are automatically updated for clients when any change is made to them on server.
    /// Cannot be static.
    /// </summary>
    [SerializeField] [SyncVar] internal TurnPhase turnPhase;

    /// <summary> List of players PlayerInGame objects </summary>
    [SerializeField] internal List<GameObject> playersObjects = new List<GameObject>();
    /// <summary>
    /// Index of PlayerInGame object from list above. 
    /// PlayerInGame object under this index is object of player that currently has a turn.
    /// It is being set once game begin.
    /// </summary>
    [SerializeField] [SyncVar] internal int activePlayerIndex = -1;

    /// <summary> Number of players who were in lobby when game started </summary>
    [SerializeField] [SyncVar] internal int connectedPlayers = 0;

    [SerializeField] [SyncVar] internal int readyPlayers = 0;

    /// <summary>
    /// Players can use cards without waiting for previous ones take an effect.
    /// This list is a queue which this mechanic uses to play cards in correct order.
    /// </summary>
    [SerializeField] internal List<GameObject> cardsUsageQueue = new List<GameObject>();
    [SerializeField] internal List<GameObject> turnActiveffects = new List<GameObject>();
    [SerializeField] internal List<GameObject> buyoutGoals = new List<GameObject>();

    //      Fighting        //
    /// <summary>
    /// NetworkInstanceId of currently fighting player.
    /// Normally such variable would have "null" value, since no players are fighting at the very begging.
    /// Unfotunately we have to use "NetworkInstanceId.Invalid" instead to avoid unintended behaviour.
    /// </summary>
    [SyncVar] internal NetworkInstanceId fightingPlayerNetId = NetworkInstanceId.Invalid;

    /// <summary>
    /// Describes if fight takes place at this time.
    /// </summary>
    [SerializeField] [SyncVar] internal bool fightInProggres;
    [SerializeField] [SyncVar] internal bool foughtInThisRound;

    [SerializeField] internal List<GameObject> fightingMonsters = new List<GameObject>();
    [SerializeField] [SyncVar] internal int fightingMonstersLevel;

    [SerializeField] [SyncVar] internal int fightingPlayersLevel;
    [SerializeField] [SyncVar] internal int fightingPlayerLevel;
    [SerializeField] internal List<Card> fightingPlayerEffects;

    /// <summary> List of players PlayerInGame scripts that are offering help to fighting player. </summary>
    [SerializeField] internal List<PlayerInGame> offeringHelpPlayers = new List<PlayerInGame>();

    /// <summary> NetworkInstanceId of help-offering player which fighting player has chosen to help him. </summary>
    [SyncVar] internal NetworkInstanceId helpingPlayerNetId = NetworkInstanceId.Invalid;
    [SerializeField] [SyncVar] internal int helpingPlayerLevel;
    [SerializeField] internal List<Card> helpingPlayerEffects;

    // Method that is run when this object is enabled in scene for first time.
    private void Start()
    {
        singleton = this;
        serverDecksManager = FindObjectOfType<DecksManager>();
        CustomNetworkManager = CustomNetManager.singleton;
        /// This is method that is essential for reconnection.
        /// I needed some way to wait for synchronization, to avoid errors when adding player BEFORE synchronization occurs.
        /// Unfortunately Unity built-in network events happen before synchronization, so they are not good for adding player.
        /// This method is called here becouse, upon reconnection, this object will be enabled on client AFTER synchronization with server,
        /// so we can add player without causing errors. 
        CustomNetworkManager.AddPlayerOnReconnect();
    }

    /// <summary>
    /// [Server] decorator - tells Unity that this method should be run only on server.
    /// Method called on server to increase number of players that have connected after changing From TitleScene to GameScene.
    /// When all players have changed scene, we assign them to players list in same order as they were arranged in lobby.
    /// Check "isServerBusy" variable in "CustomNetworkManager" for more information.
    /// </summary>
    [Server]
    internal IEnumerator ReportPresence()
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        connectedPlayers++;

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;

        if (connectedPlayers == CustomNetManager.playersToConnect)
            StartCoroutine(ServerArrangePlayersList());
    }

    /// <summary> Assigning players to list in same order as they were arranged in lobby. </summary>
    [Server]
    IEnumerator ServerArrangePlayersList()
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        //Debug.Log("Players To Connect - " + CustomNetworkManager.playersToConnect);
        //Debug.Log("Connected Players - " + connectedPlayers);

        PlayerManager[] players = FindObjectsOfType<PlayerManager>();

        for (int i = 0; i < connectedPlayers; i++)
        {
            for (int j = 0; j < connectedPlayers; j++)
            {
                if (players[j].playerIndex == i)
                {
                    RpcAddToPlayersList(players[j].PlayerInGame.GetComponent<PlayerInGame>().netId);
                    yield return new WaitForEndOfFrame();
                    break;
                }
            }
        }

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }

    /// <summary> [ClientRpc] - decorator for methods that are called exculisvely BY SERVER and ran only ON CLIENTS. </summary>
    /// <param name="connectedPlayersNetId"> NetworkInstanceId of player that has to be added to list next. </param>
    [ClientRpc]
    void RpcAddToPlayersList( NetworkInstanceId connectedPlayersNetId )
    {
        playersObjects.Add(ClientScene.FindLocalObject(connectedPlayersNetId));
    }

    /// <summary>
    /// [Command] - decorator for methods that are CALLED exclusively BY CLIENTS, and RUN only ON SERVER.
    /// Only objects over which players have authority can call [Command] methods.
    /// 
    /// ADDITIONAL INFO: 
    /// So in this case, since no client will ever have authority over ServerGameManager(this) object,
    /// the CmdShuffleDecks() method will never work. 
    /// ( Unless called on host which is client AND server. Server has authority over any object without authority by default. )
    /// </summary>
    [Command]
    public void CmdShuffleDecks()
    {
        serverDecksManager.ShuffleDecks();
    }

    /// <summary>
    /// Called when player presses "ProgressButton".
    /// If all players are ready game progresses to next phase.
    /// </summary>
    [Server]
    internal void ReadyPlayersUp()
    {
        //Debug.Log("ReadyPlayersUp()");
        this.readyPlayers++;

        if (readyPlayers == playersObjects.Count)
        {
            switch (GamePhase)
            {
                case GamePhase.PreStart:
                    Debug.Log("Beginning game!");
                    ServerBeginGame();
                    break;
                case GamePhase.InProgress:
                    Debug.Log("Game In Progress!");
                    ProgressTurn();
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Method called when all players are ready for first time.
    /// Synchronizes objects with loaded game or starts new game - sends new set of cards to all players, and starts turn for first player.
    /// </summary>
    [Server] // Called on server
    void ServerBeginGame()
    {
        Debug.Log("Game Begins!");
        GamePhase = GamePhase.InProgress;
        if (CustomNetManager.gameLoaded)
        {
            StartCoroutine(SaveSystem.LoadGame());
        }
        else
        {
            foreach (var player in playersObjects)
            {
                player.GetComponent<PlayerInGame>().SendStartingHand();
            }

            ServerNewTurn();
        }
    }

    /// <summary>
    /// Sets variables on server for new turn.
    /// Tells players that new turn has started.
    /// Tells player with turn that his turn has started and enables his functionality for having turn.
    /// AutoSaves game.
    /// </summary>
    [Server]
    void ServerNewTurn() // Setting vars for new turn
    {
        if (activePlayerIndex >= 0)
            playersObjects[activePlayerIndex].GetComponent<PlayerInGame>().hasTurn = false;

        turnPhase = TurnPhase.Beginning;
        foughtInThisRound = false;

        foreach (var effect in turnActiveffects)
        {
            StartCoroutine(effect.GetComponent<Effect>().ServerOnTurnEnd());
        }

        turnActiveffects.Clear();

        activePlayerIndex = (activePlayerIndex + 1) % playersObjects.Count;
        PlayerInGame playerWithTurn = playersObjects[activePlayerIndex].GetComponent<PlayerInGame>();
        playerWithTurn.hasTurn = true;
        StartCoroutine(playerWithTurn.ServerPersonalAlertCoroutine("Your Turn Now!"));

        PlayerInGame.ServerNewTurn();
        StartCoroutine(ServerTurnOwnerReadiness());
        StartCoroutine(ServerAlert("New Turn Starts!"));
        SaveSystem.AutoSaveGame();
    }

    /// <summary>
    /// Called in ReadyPlayersUp() when GamePhase is InProgress.
    /// Ends fight and switches turnPhase.
    /// Ends Turns Beginning Phase, switches to Turns Search Phase and makes active player draw 3 doors to choose.
    /// </summary>
    [Server]
    internal void ProgressTurn()
    {
        if (fightInProggres)
        {
            ServerEndFight(true);
            return;
        }

        if (turnPhase == TurnPhase.Beginning)
        {
            Debug.Log("Progressing turn phase - Beginning => Search!");
            ServerAlert("Turn phase: Search");
            StartCoroutine(playersObjects[activePlayerIndex].GetComponent<PlayerInGame>().ServerSendCards(Deck.Doors, 3, true, true));
            turnPhase = TurnPhase.Search;
        }
    }

    /// <summary>
    /// Method that makes server wait only for turn owner to be ready.
    /// It accomplishes that by setting readyPlayers to be 1 value less than number of players and then enables "ProgressButton" only for turn owner.
    /// 
    /// readyPlayers is [SyncVar] and RpcTurnOwnerReadiness() is [ClientRpc] method.
    /// To keep intended network behaviour I wait 1 frame in between calling them, so that they are called in separate frames.
    /// For more information check isServerBusy variable in CustomNetworkManager class.
    /// </summary>
    [Server]
    internal IEnumerator ServerTurnOwnerReadiness()
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        readyPlayers = playersObjects.Count - 1;
        yield return new WaitForEndOfFrame();

        playersObjects[activePlayerIndex].GetComponent<PlayerInGame>().RpcTurnOwnerReadiness();

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }

    /// <summary>
    /// Method that makes server wait all players to be ready.
    /// Sets readyPlayers to 0 and enables "ProgressButton" for all players.
    /// </summary>
    [Server]
    internal IEnumerator ServerAllPlayersReadiness()
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        readyPlayers = 0;
        yield return new WaitForEndOfFrame();

        playersObjects[0].GetComponent<PlayerInGame>().RpcAllPlayersReadiness();

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }

    /// <summary> Updates levels of fighting players. </summary>
    [Server]
    internal void ServerUpdateFightingPlayersLevel()
    {
        if (!fightInProggres)
            return;

        PlayerInGame playerScript = ClientScene.FindLocalObject(fightingPlayerNetId).GetComponent<PlayerInGame>();
        fightingPlayerLevel = playerScript.Level;

        foreach (var item in playerScript.equippedItems)
            fightingPlayerLevel += item.GetComponent<EquipmentCard>().cardValues.level;

        foreach (var effect in fightingPlayerEffects)
            fightingPlayerLevel += effect.level;

        fightingPlayersLevel = fightingPlayerLevel;

        if (helpingPlayerNetId != NetworkInstanceId.Invalid)
        {
            playerScript = ClientScene.FindLocalObject(helpingPlayerNetId).GetComponent<PlayerInGame>();
            helpingPlayerLevel = playerScript.Level;

            foreach (var item in playerScript.equippedItems)
                fightingPlayerLevel += item.GetComponent<EquipmentCard>().cardValues.level;

            foreach (var effect in helpingPlayerEffects)
                helpingPlayerLevel += effect.level;

            fightingPlayersLevel += helpingPlayerLevel;
        }
    }

    [Server]
    internal void ServerUpdateMonstersLevels()
    {
        fightingMonstersLevel = 0;

        foreach (var monster in fightingMonsters)
            fightingMonstersLevel += monster.GetComponent<MonsterCard>().cardValues.level;
    }

    /// <summary> Method called when fight is ended. </summary>
    /// <param name="isFightSettled"> True if fight ended normal way. False if something was used to end fight without winner.</param>
    [Server]
    internal void ServerEndFight( bool isFightSettled )
    {
        if (isFightSettled)
        {
            if (fightingPlayersLevel > fightingMonstersLevel) ServerFightWon();
            else ServerFightLost();
            foughtInThisRound = true;
        }
        else
        {
            // If fight is canceled nobody wins and player can fight again
            foughtInThisRound = false;
        }

        // Reseting variables used in fight.
        readyPlayers = 0;
        fightInProggres = false;
        fightingPlayerNetId = NetworkInstanceId.Invalid;
        helpingPlayerNetId = NetworkInstanceId.Invalid;
        PlayerInGame.localPlayerInGame.EndFight();
        StartCoroutine(ServerTurnOwnerReadiness());
    }

    /// <summary> Called when players won the fight. </summary>
    [Server]
    void ServerFightWon()
    {
        int treasuresToDrawCount = 0;
        short levelsGained = 0;

        // Counting how many treasures and levels slayed monsters grant.
        foreach (var monster in fightingMonsters)
        {
            MonsterValue monVars = (MonsterValue)monster.GetComponent<MonsterCard>().cardValues;
            treasuresToDrawCount += monVars.treasuresCount;
            levelsGained += monVars.levelsToGrant;
        }
        // Finding fighting players PlayerInGame script.
        PlayerInGame fightingPlayer = ClientScene.FindLocalObject(fightingPlayerNetId).GetComponent<PlayerInGame>();
        fightingPlayer.FightWon(treasuresToDrawCount, levelsGained);
        StartCoroutine(ServerAlert(fightingPlayer.nickName + " won the battle!"));

        ServerApplyMonstersTriumphEffects();
    }

    /// <summary> Called when players lost the battle. </summary>
    [Server]
    void ServerFightLost()
    {
        // Finding fighting player PlayerInGame script.
        PlayerInGame fightingPlayer = ClientScene.FindLocalObject(fightingPlayerNetId).GetComponent<PlayerInGame>();
        StartCoroutine(ServerAlert(fightingPlayer.nickName + " lost the battle!"));

        ServerApplyMonstersDefeatEffects();
    }

    [Server]
    void ServerApplyMonstersDefeatEffects()
    {
        foreach (var monster in fightingMonsters)
        {
            StartCoroutine(ServerApplyMonsterDefeatEffect(monster));
        }
    }

    [Server]
    void ServerApplyMonstersTriumphEffects()
    {
        foreach (var monster in fightingMonsters)
        {
            StartCoroutine(ServerApplyMonsterTriumphEffect(monster));
        }
    }

    [Server]
    IEnumerator ServerApplyMonsterDefeatEffect( GameObject monster )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        monster.GetComponent<MonsterCard>().DefeatEffect(fightingPlayerNetId);

        if (helpingPlayerNetId != NetworkInstanceId.Invalid)
            monster.GetComponent<MonsterCard>().DefeatEffect(helpingPlayerNetId);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }

    [Server]
    IEnumerator ServerApplyMonsterTriumphEffect( GameObject monster )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        monster.GetComponent<MonsterCard>().TriumphEffect(fightingPlayerNetId);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }

    /// <summary> Ending turn and starting new one </summary>
    [Server]
    internal void ServerEndTurn()
    {
        Debug.Log("ServerGameManager.EndTurn()");

        ServerNewTurn();
    }

    /// <summary> Called and ran only on server </summary>
    /// <param name="alertText"> Text for alert to contain. </param>
    [Server]
    internal IEnumerator ServerAlert( string alertText )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        RpcAlert(alertText);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }

    /// <summary>Called only by server, ran only on clients</summary>
    /// <param name="text"> Text for alert to contain. </param>
    [ClientRpc]
    void RpcAlert( string text )
    {
        //Creates Alert object containing text. Object disappears after a while or when clicked.
        InfoPanel.Alert(text);
    }

    // Function for searching card by its name. Essential when loading game.
    internal static GameObject GetCardByName( string name )
    {
        return singleton.serverDecksManager.GetCardByName(name);
    }
}
