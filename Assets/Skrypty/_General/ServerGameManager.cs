﻿#pragma warning disable CS0618 // Typ lub składowa jest przestarzała
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
    [SerializeField] private ServerDecksManager serverDecksManager;
    internal ServerDecksManager ServerDecksManager { get => serverDecksManager; }

    //      Turn and Players Vars       //
    [SerializeField] [SyncVar] internal TurnPhase turnPhase;
    [SerializeField] GamePhase GamePhase = GamePhase.PreStart;

    [SerializeField] internal List<GameObject> playersObjects = new List<GameObject>();
    [SyncVar] [SerializeField] internal int activePlayerIndex = -1;

    [SyncVar] internal int connectedPlayers = 0;
    [SyncVar] internal int readyPlayers = 0;


    //      Fighting        //
    [SyncVar] internal NetworkInstanceId activePlayerNetId = NetworkInstanceId.Invalid;
    [SyncVar] internal NetworkInstanceId fightingPlayerNetId = NetworkInstanceId.Invalid;

    [SerializeField] internal List<PlayerInGame> offeringHelpPlayers = new List<PlayerInGame>();
    [SerializeField] [SyncVar] internal NetworkInstanceId helpingPlayerNetId = NetworkInstanceId.Invalid;

    internal List<GameObject> fightingMonsters = new List<GameObject>();

    [SerializeField] [SyncVar] internal int fightingMonstersLevel;
    [SerializeField] [SyncVar] internal short fightingPlayerLevel;
    [SerializeField] [SyncVar] internal short helpingPlayerLevel;
    [SerializeField] [SyncVar] internal short fightingPlayersLevel;
    //[SerializeField] [SyncVar] int availableTreasures = 0;

    [SerializeField] [SyncVar] internal bool fightInProggres;
    [SyncVar] internal bool foughtInThisRound;
    [SyncVar] internal bool canPlayersTrade = true;
    //[SyncVar] internal bool canPlayersEquip = true;

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
    internal IEnumerator ReportPresence( NetworkInstanceId connectedPlayersNetId )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        this.connectedPlayers++;
        RpcReportPresence(connectedPlayersNetId);
    }

    [ClientRpc]
    void RpcReportPresence( NetworkInstanceId connectedPlayersNetId )
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

        if (readyPlayers == connectedPlayers)
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

        foreach (var player in playersObjects)
        {
            player.GetComponent<PlayerInGame>().SendStartingHand();
        }

        NewTurnSet();
    }

    [Server] // Called on server
    void NewTurnSet() // Setting vars for new turn
    {
        turnPhase = TurnPhase.Beginning;

        if (activePlayerIndex >= 0)
            playersObjects[activePlayerIndex].GetComponent<PlayerInGame>().hasTurn = false;

        activePlayerIndex = (activePlayerIndex + 1) % playersObjects.Count;
        PlayerInGame playerWithTurn = playersObjects[activePlayerIndex].GetComponent<PlayerInGame>();
        playerWithTurn.hasTurn = true;
        StartCoroutine(playerWithTurn.ServerPersonalAlertCoroutine("You Turn Now!"));

        activePlayerNetId = playersObjects[activePlayerIndex].GetComponent<NetworkIdentity>().netId;

        TurnOwnerReadiness();
        playersObjects[activePlayerIndex].GetComponent<PlayerInGame>().StartTurn();
        StartCoroutine(ServerAlert("New Turn Starts!"));
    }

    [Server]
    internal void ProgressTurn()
    {
        if (fightInProggres)
        {
            EndFight();
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
    IEnumerator ServerTurnOwnerReadiness()
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;
        readyPlayers = playersObjects.Count - 1;
        playersObjects[activePlayerIndex].GetComponent<PlayerInGame>().RpcTurnOwnerReadiness();
    }

    [Server]
    internal void TurnOwnerReadiness()
    {
        StartCoroutine(ServerTurnOwnerReadiness());
    }

    [Server]
    internal IEnumerator ServerAllPlayersReadiness()
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;
        readyPlayers = 0;
        playersObjects[0].GetComponent<PlayerInGame>().RpcAllPlayersReadiness();
    }

    [Server]
    internal void AllPlayersReadiness()
    {
        StartCoroutine(ServerAllPlayersReadiness());
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
    void EndFight() // PowerCheck, Win or Lose -Effect, change bools to non-fight state
    {
        if (fightingPlayersLevel > fightingMonstersLevel) FightWon();
        else FightLost();
        readyPlayers = 0;
        fightInProggres = false;
        foughtInThisRound = true;
        canPlayersTrade = true;
        fightingPlayerNetId = NetworkInstanceId.Invalid;
        helpingPlayerNetId = NetworkInstanceId.Invalid;
        PlayerInGame.localPlayerInGame.EndFight();
        TurnOwnerReadiness();
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

        StartCoroutine(ApplyMonstersDefeatEffects());
    }

    [Server]
    IEnumerator ApplyMonstersDefeatEffects()
    {
        foreach (var monster in fightingMonsters)
        {
            if (CustomNetworkManager.isServerBusy)
                yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
            CustomNetworkManager.isServerBusy = true;

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
    }

    [Server]
    IEnumerator ApplyMonsterTriumphEffect( GameObject monster )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        monster.GetComponent<MonsterCard>().TriumphEffect(fightingPlayerNetId);
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
    }

    [ClientRpc]
    void RpcAlert( string text )
    {
        InfoPanel.Alert(text);
    }

    void LoadGame()
    {
        SaveGameData data = SaveSystem.LoadGame();

        if (data.playerXLevel.Count != playersObjects.Count)
            Debug.LogError("Couldn't load the game. Number of Players Doesn't match");

        for (int i = 0; i < playersObjects.Count; i++)
        {
            PlayerInGame playerScript = playersObjects[i].GetComponent<PlayerInGame>();
            playerScript.Level = data.playerXLevel[i];
            playerScript.isPlayerAlive = data.isPlayerXAlive[i];

            foreach (var card in data.cardsInXHand[i])
            {
                serverDecksManager.SpawnCardByName(card, playerScript);
            }

            GameObject eqPiece = serverDecksManager.SpawnCardByName(data.playerXhead[i], playerScript);
            if (eqPiece)
                StartCoroutine(playerScript.ServerEquip(eqPiece.GetComponent<Card>().netId));
            eqPiece = null;

            eqPiece = serverDecksManager.SpawnCardByName(data.playerXchest[i], playerScript);
            if (eqPiece)
                StartCoroutine(playerScript.ServerEquip(eqPiece.GetComponent<Card>().netId));
            eqPiece = null;

            eqPiece = serverDecksManager.SpawnCardByName(data.playerXhands[i], playerScript);
            if (eqPiece)
                StartCoroutine(playerScript.ServerEquip(eqPiece.GetComponent<Card>().netId));
            eqPiece = null;

            eqPiece = serverDecksManager.SpawnCardByName(data.playerXlegs[i], playerScript);
            if (eqPiece)
                StartCoroutine(playerScript.ServerEquip(eqPiece.GetComponent<Card>().netId));
            eqPiece = null;

            eqPiece = serverDecksManager.SpawnCardByName(data.playerXfeet[i], playerScript);
            if (eqPiece)
                StartCoroutine(playerScript.ServerEquip(eqPiece.GetComponent<Card>().netId));
            eqPiece = null;

            eqPiece = serverDecksManager.SpawnCardByName(data.playerXring[i], playerScript);
            if (eqPiece)
                StartCoroutine(playerScript.ServerEquip(eqPiece.GetComponent<Card>().netId));
            eqPiece = null;

            eqPiece = serverDecksManager.SpawnCardByName(data.playerXweapon1[i], playerScript);
            if (eqPiece)
                StartCoroutine(playerScript.ServerEquip(eqPiece.GetComponent<Card>().netId));
            eqPiece = null;

            eqPiece = serverDecksManager.SpawnCardByName(data.playerXweapon2[i], playerScript);
            if (eqPiece)
                StartCoroutine(playerScript.ServerEquip(eqPiece.GetComponent<Card>().netId));
        }

        foreach (var card in data.cardsInDoorsDeck)
        {
            serverDecksManager.SpawnCardByName(card);
        }

        foreach (var card in data.cardsInTreasuresDeck)
        {
            serverDecksManager.SpawnCardByName(card);
        }

        foreach (var card in data.cardsInSpellsDeck)
        {
            serverDecksManager.SpawnCardByName(card);
        }

        foreach (var card in data.cardsInHelpHandsDeck)
        {
            serverDecksManager.SpawnCardByName(card);
        }
    }
}
