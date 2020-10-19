#pragma warning disable CS0618 // Typ lub składowa jest przestarzała
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public enum Deck { Doors, Treasures, HelpingHand, Spells, DiscardedDoors, DiscardedSpells, DiscardedTreasures, DiscardedHelpingHand };
public enum TurnPhase { Beginning, Search };
enum GamePhase { PreStart, InProgress };
public enum Entity { Player, Monster, Both };
public enum TreasureType { Equipment, Buff };
public enum EqPart { Head, Chest, Hands, Legs, Feet, Ring, Weapon1, Weapon2 };

public class ServerGameManager : NetworkBehaviour
{
    //      Stored References       //
    internal static ServerGameManager serverGameManager;
    CustomNetworkManager CustomNetworkManager;
    private static ServerDecksManager serverDecksManager;
    internal ServerDecksManager ServerDecksManager { get => serverDecksManager; }

    //      Turn and Players Vars       //
    [SerializeField] [SyncVar] internal TurnPhase turnPhase;
    [SerializeField] GamePhase GamePhase = GamePhase.PreStart;

    [SerializeField] internal List<GameObject> playersObjects = new List<GameObject>();
    [SerializeField] [SyncVar] internal int activePlayerIndex = -1;

    [SyncVar] internal int connectedPlayers = 0;
    [SyncVar] internal int readyPlayers = 0;

    [SerializeField] internal List<GameObject> StoredCardUsesToConfirm = new List<GameObject>();

    //      Fighting        //
    [SyncVar] internal NetworkInstanceId activePlayerNetId = NetworkInstanceId.Invalid;
    [SyncVar] internal NetworkInstanceId fightingPlayerNetId = NetworkInstanceId.Invalid;

    [SerializeField] internal List<PlayerInGame> offeringHelpPlayers = new List<PlayerInGame>();
    [SerializeField] [SyncVar] internal NetworkInstanceId helpingPlayerNetId = NetworkInstanceId.Invalid;

    [SerializeField] [SyncVar] internal bool fightInProggres;

    internal List<GameObject> fightingMonsters = new List<GameObject>();

    [SerializeField] [SyncVar] internal int fightingMonstersLevel;

    [SerializeField] [SyncVar] internal short fightingPlayerLevel;
    [SerializeField] [SyncVar] internal short helpingPlayerLevel;
    [SerializeField] [SyncVar] internal short fightingPlayersLevel;

    [SyncVar] internal bool foughtInThisRound;

    private void Start()
    {
        serverGameManager = this;
        serverDecksManager = FindObjectOfType<ServerDecksManager>();
        CustomNetworkManager = CustomNetworkManager.customNetworkManager;
        SceneManager.sceneLoaded += InitializeInGameScene;
    }

    private void InitializeInGameScene( Scene scene, LoadSceneMode mode )
    {

    }

    [Command]
    internal void CmdChangeMonsterLevelBy( int value )
    {
        fightingMonstersLevel += value;
    }

    [Server]
    internal IEnumerator ReportPresence()
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        connectedPlayers++;

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;

        if (connectedPlayers == CustomNetworkManager.playersToConnect)
            StartCoroutine(ServerArrangePlayersList());
    }

    [Server]
    IEnumerator ServerArrangePlayersList()
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        Debug.Log("Players To Connect - " + CustomNetworkManager.playersToConnect);
        Debug.Log("Connected Players - " + connectedPlayers);

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

    [ClientRpc]
    void RpcAddToPlayersList( NetworkInstanceId connectedPlayersNetId )
    {
        playersObjects.Add(ClientScene.FindLocalObject(connectedPlayersNetId));
    }

    [Server]
    internal void SetMonsterLevelTo( int value )
    {
        fightingMonstersLevel = value;
    }

    [Command]
    public void CmdShuffleDecks()
    {
        serverDecksManager.ShuffleDecks();
    }

    [Server]
    internal void ReadyPlayersUp()
    {
        this.readyPlayers++;

        if (readyPlayers == playersObjects.Count)
        {
            switch (GamePhase)
            {
                case GamePhase.PreStart:
                    Debug.Log("Beginning game!");
                    BeginGame();
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

    [Server] // Called on server
    void BeginGame()
    {
        Debug.Log("Game Begins!");
        GamePhase = GamePhase.InProgress;
        if (CustomNetworkManager.gameLoaded)
        {
            StartCoroutine(SaveSystem.LoadGame());
        }
        else
        {
            foreach (var player in playersObjects)
            {
                player.GetComponent<PlayerInGame>().SendStartingHand();
            }

            NewTurnSet();
        }
    }

    [Server] // Called on server
    void NewTurnSet( TurnPhase turnPhase = TurnPhase.Beginning ) // Setting vars for new turn
    {
        this.turnPhase = turnPhase;

        if (activePlayerIndex >= 0)
            playersObjects[activePlayerIndex].GetComponent<PlayerInGame>().hasTurn = false;

        activePlayerIndex = (activePlayerIndex + 1) % playersObjects.Count;
        PlayerInGame playerWithTurn = playersObjects[activePlayerIndex].GetComponent<PlayerInGame>();
        playerWithTurn.hasTurn = true;
        StartCoroutine(playerWithTurn.ServerPersonalAlertCoroutine("Your Turn Now!"));

        activePlayerNetId = playersObjects[activePlayerIndex].GetComponent<NetworkIdentity>().netId;

        StartCoroutine(ServerTurnOwnerReadiness());
        playersObjects[activePlayerIndex].GetComponent<PlayerInGame>().StartTurn();
        StartCoroutine(ServerAlert("New Turn Starts!"));
    }

    [Server]
    internal void ProgressTurn()
    {
        if (fightInProggres)
        {
            EndFight(true);
            return;
        }

        switch (turnPhase)
        {
            case TurnPhase.Beginning: // Player draws and chooses Doors
                Debug.Log("Progressing turn phase - Beginning => Search!");
                ServerAlert("Turn phase: Search");
                StartCoroutine(playersObjects[activePlayerIndex].GetComponent<PlayerInGame>().ServerSendCardsCoroutine(Deck.Doors, 3, true, true));
                turnPhase = TurnPhase.Search;
                break;
            default:
                break;
        }
    }

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

    [Server]
    internal void UpdateFightingPlayersLevel()
    {
        if (!fightInProggres)
            return;

        fightingPlayersLevel = fightingPlayerLevel = ClientScene.FindLocalObject(fightingPlayerNetId).GetComponent<PlayerInGame>().Level;

        if (helpingPlayerNetId != NetworkInstanceId.Invalid)
            fightingPlayersLevel += helpingPlayerLevel = ClientScene.FindLocalObject(helpingPlayerNetId).GetComponent<PlayerInGame>().Level;
    }

    [Server]
    internal void EndFight( bool isFightSettled ) // PowerCheck, Win or Lose -Effect, change bools to non-fight state
    {
        if (isFightSettled) // Fight has a winner, so its settled and correct effect should occur
        {
            if (fightingPlayersLevel > fightingMonstersLevel) FightWon();
            else FightLost();
            foughtInThisRound = true;
        }
        else
        {
            foughtInThisRound = false; // If fight is canceled nobody wins and player can fight again
        }

        readyPlayers = 0;
        fightInProggres = false;
        fightingPlayerNetId = NetworkInstanceId.Invalid;
        helpingPlayerNetId = NetworkInstanceId.Invalid;
        PlayerInGame.localPlayerInGame.EndFight();
        StartCoroutine(ServerTurnOwnerReadiness());
    }

    [Server]
    void FightWon()
    {
        int treasuresToDrawCount = 0;
        short levelsGained = 0;

        foreach (var monster in fightingMonsters)
        {
            MonsterValue monVars = (MonsterValue)monster.GetComponent<MonsterCard>().cardValues;
            treasuresToDrawCount += monVars.treasuresCount;
            levelsGained += monVars.levelsToGrant;
        }
        PlayerInGame fightingPlayer = ClientScene.FindLocalObject(fightingPlayerNetId).GetComponent<PlayerInGame>();
        fightingPlayer.FightWon(treasuresToDrawCount, levelsGained);
        StartCoroutine(ServerAlert(fightingPlayer.NickName + " won the battle!"));

        StartCoroutine(ApplyMonstersTriumphEffects());
    }

    [Server]
    void FightLost()
    {
        PlayerInGame fightingPlayer = ClientScene.FindLocalObject(fightingPlayerNetId).GetComponent<PlayerInGame>();
        StartCoroutine(ServerAlert(fightingPlayer.NickName + " lost the battle!"));

        ApplyMonstersDefeatEffects();
    }

    [Server]
    void ApplyMonstersDefeatEffects()
    {
        foreach (var monster in fightingMonsters)
        {
            StartCoroutine(ApplyMonsterDefeatEffect(monster));
        }
    }

    [Server]
    IEnumerator ApplyMonstersTriumphEffects()
    {
        foreach (var monster in fightingMonsters)
        {
            if (CustomNetworkManager.isServerBusy)
                yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
            CustomNetworkManager.isServerBusy = true;

            StartCoroutine(ApplyMonsterTriumphEffect(monster));

            yield return new WaitForEndOfFrame();
            CustomNetworkManager.isServerBusy = false;
        }
    }

    [Server]
    IEnumerator ApplyMonsterDefeatEffect( GameObject monster )
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
    IEnumerator ApplyMonsterTriumphEffect( GameObject monster )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        monster.GetComponent<MonsterCard>().TriumphEffect(fightingPlayerNetId);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }

    internal void EndTurn()
    {
        Debug.Log("ServerGameManager.EndTurn()");
        NewTurnSet();
    }

    [Server]
    IEnumerator ServerAlert( string alertText )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        RpcAlert(alertText);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }

    [ClientRpc]
    void RpcAlert( string text )
    {
        InfoPanel.Alert(text);
    }

    internal static GameObject GetCardByName( string name )
    {
        return serverDecksManager.GetCardByName(name);
    }
}
