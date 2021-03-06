﻿#pragma warning disable CS0618 // Type too old lul

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

enum PlayerColors { Green, Blue, Red, Brown, SkyBlue, Black, White, Purple, Orange, Pink }
/// <summary>
/// Class for managing player connection in Lobby.
/// </summary>
public class LobbyManager : NetworkBehaviour
{
    static internal LobbyManager lobbyManager;

    [SerializeField] private StartGameButtonInLobby startGameButton;
    [SerializeField] [SyncVar] private int readyPlayers = 0;
    [SerializeField] [SyncVar] internal int connectedPlayers = 0;

    static List<string> loadedPlayersNames = new List<string>();

    // Colors of players based on their position in lobby.
    static internal Color[] Colors =
    {
        new Color(0, 255, 0, 255),
        new Color(0, 155, 255, 255),
        new Color(255, 52, 52, 255),
        new Color(183, 89, 0, 255),
        new Color(0, 255, 255, 255),
        new Color(50, 50, 50, 255),
        new Color(250, 250, 250, 255),
        new Color(150, 0, 150, 255),
        new Color(255, 115, 19, 255),
        new Color(255, 90, 255, 255)
    };

    static internal PILLoadHeader[] PILLoadHeaders;

    private void Start()
    {
        PILLoadHeaders = new PILLoadHeader[]
    {
        MainMenu.lobbyPanel.Find("LoadHeader").GetComponent<PILLoadHeader>(),
        MainMenu.lobbyPanel.Find("LoadHeader (1)").GetComponent<PILLoadHeader>(),
        MainMenu.lobbyPanel.Find("LoadHeader (2)").GetComponent<PILLoadHeader>(),
        MainMenu.lobbyPanel.Find("LoadHeader (3)").GetComponent<PILLoadHeader>(),
        MainMenu.lobbyPanel.Find("LoadHeader (4)").GetComponent<PILLoadHeader>(),
        MainMenu.lobbyPanel.Find("LoadHeader (5)").GetComponent<PILLoadHeader>(),
        MainMenu.lobbyPanel.Find("LoadHeader (6)").GetComponent<PILLoadHeader>(),
        MainMenu.lobbyPanel.Find("LoadHeader (7)").GetComponent<PILLoadHeader>(),
        MainMenu.lobbyPanel.Find("LoadHeader (8)").GetComponent<PILLoadHeader>(),
        MainMenu.lobbyPanel.Find("LoadHeader (9)").GetComponent<PILLoadHeader>()
    };

        lobbyManager = this;
    }

    [Server]
    internal static void Initialize()
    {
        lobbyManager.readyPlayers = 0;
        lobbyManager.connectedPlayers = 0;
    }
    public static void ReadyPlayer()
    {
        lobbyManager.readyPlayers += 1;
        ReadyCheck();
    }
    public static void UnreadyPlayer()
    {
        lobbyManager.readyPlayers -= 1;
        ReadyCheck();
    }
    public static void UpdateConnectedPlayers()
    {
        lobbyManager.connectedPlayers = NetworkServer.connections.Count;
        ReadyCheck();
    }
    public static void ConnectedPlayersDown()
    {
        if (!SceneManager.GetActiveScene().name.Equals("TitleScene"))
            return;
        lobbyManager.connectedPlayers -= 1;
        ReadyCheck();
    }
    /// <summary>
    /// Enable Start Game Button if all connected players are ready to play OR if all players needed to load game are connected and ready.
    /// Otherwise disable button.
    /// </summary>
    static void ReadyCheck()
    {
        if ((lobbyManager.readyPlayers == lobbyManager.connectedPlayers &&
            LobbyPlayersCounter.numOfLoadedPlayers == 0) ||
            (LobbyPlayersCounter.numOfLoadedPlayers != 0 &&
            lobbyManager.readyPlayers == lobbyManager.connectedPlayers &&
            lobbyManager.readyPlayers == LobbyPlayersCounter.numOfLoadedPlayers))
        {
            lobbyManager.startGameButton.EnableStartGameButton();
        }
        else
            lobbyManager.startGameButton.DisableStartGameButton();
    }
    /// <summary>
    /// Calling updating Players Counter UI on server.
    /// </summary>
    [Server]
    internal static IEnumerator ServerUpdatePlayersCounter()
    {
        if (CustomNetManager.singleton.isServerBusy)
            yield return new WaitUntil(() => !CustomNetManager.singleton.isServerBusy);
        CustomNetManager.singleton.isServerBusy = true;

        lobbyManager.RpcUpdatePlayersCounter(LobbyPlayersCounter.numOfLoadedPlayers);

        yield return new WaitForEndOfFrame();
        CustomNetManager.singleton.isServerBusy = false;
    }
    /// <summary>
    /// Updating Players Counter  UI on Clients.
    /// </summary>
    [ClientRpc]
    internal void RpcUpdatePlayersCounter( int numOfLoadedPlayers )
    {
        LobbyPlayersCounter.numOfLoadedPlayers = numOfLoadedPlayers;
        LobbyPlayersCounter.UpdatePlayersCounter(readyPlayers, connectedPlayers);
    }
    /// <summary>
    /// Activating headers of loaded players.
    /// </summary>
    [Server]
    internal static IEnumerator ServerActivateLoadHeaders()
    {
        if (CustomNetManager.singleton.isServerBusy)
            yield return new WaitUntil(() => !CustomNetManager.singleton.isServerBusy);
        CustomNetManager.singleton.isServerBusy = true;

        lobbyManager.RpcInitGetPlayerNames();
        yield return new WaitForEndOfFrame();

        int limit = LobbyPlayersCounter.numOfLoadedPlayers;
        for (int i = 0; i < limit; i++)
        {
            lobbyManager.RpcGetPlayerName(SaveSystem.loadedSave.playersData[i].nickName);
            yield return new WaitForEndOfFrame();
        }
        lobbyManager.RpcUpdatePlayersNamesInHeaders();

        yield return new WaitForEndOfFrame();
        CustomNetManager.singleton.isServerBusy = false;
    }
    [ClientRpc]
    void RpcInitGetPlayerNames()
    {
        //Debug.Log("RpcInitGetPlayerNames()");
        loadedPlayersNames.Clear();
    }
    [ClientRpc]
    void RpcGetPlayerName( string name )
    {
        //Debug.Log("RpcGetPlayerName().name - " + name);
        loadedPlayersNames.Add(name);
    }
    [ClientRpc]
    void RpcUpdatePlayersNamesInHeaders()
    {
        int limit = LobbyPlayersCounter.numOfLoadedPlayers;

        //Debug.Log("RpcUpdatePlayersNamesInHeaders().limit - " + limit);
        for (int i = 0; i < limit; i++)
            PILLoadHeaders[i].Initialize(loadedPlayersNames[i]);
    }
    [Server]
    internal static IEnumerator ServerDeactivateLoadHeaders()
    {
        if (CustomNetManager.singleton.isServerBusy)
            yield return new WaitUntil(() => !CustomNetManager.singleton.isServerBusy);
        CustomNetManager.singleton.isServerBusy = true;

        lobbyManager.RpcDeactivateLoadHeaders();

        yield return new WaitForEndOfFrame();
        CustomNetManager.singleton.isServerBusy = false;
    }
    [ClientRpc]
    void RpcDeactivateLoadHeaders()
    {
        ClientDeactivateLoadHeaders();
    }
    [Client]
    void ClientDeactivateLoadHeaders()
    {
        foreach (var pILLoadHeader in FindObjectsOfType<PILLoadHeader>())
            pILLoadHeader.gameObject.SetActive(false);
    }
}