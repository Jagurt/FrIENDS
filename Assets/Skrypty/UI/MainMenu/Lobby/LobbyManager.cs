#pragma warning disable CS0618 // Type too old lul

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

enum PlayerColors { Green, Blue, Red, Brown, SkyBlue, Black, White, Purple, Orange, Pink }

public class LobbyManager : NetworkBehaviour
{
    static internal LobbyManager lobbyManager;

    [SerializeField] private StartGameButtonInLobby startGameButton;
    [SerializeField] [SyncVar] private int readyPlayers = 0;
    [SerializeField] [SyncVar] internal int connectedPlayers = 0;

    static List<string> playerNames = new List<string>();

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

    public static void ConnectedPlayersUpdate()
    {
        lobbyManager.connectedPlayers = NetworkServer.connections.Count;
        ReadyCheck();
    }

    public static void ConnectedPlayersDown()
    {
        lobbyManager.connectedPlayers -= 1;
        ReadyCheck();
    }

    static void ReadyCheck()
    {
        if ((lobbyManager.readyPlayers == lobbyManager.connectedPlayers &&
            LobbyPlayersCounter.numOfLoadedPlayers == 0)
            ||
            (LobbyPlayersCounter.numOfLoadedPlayers != 0 &&
            lobbyManager.readyPlayers == lobbyManager.connectedPlayers &&
            lobbyManager.readyPlayers == LobbyPlayersCounter.numOfLoadedPlayers))
        {
            lobbyManager.startGameButton.EnableStartGameButton();
        }
        else
            lobbyManager.startGameButton.DisableStartGameButton();
    }

    [Server]
    internal static IEnumerator ServerUpdatePlayersCounter()
    {
        if (CustomNetworkManager.customNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.customNetworkManager.isServerBusy);
        CustomNetworkManager.customNetworkManager.isServerBusy = true;

        lobbyManager.RpcUpdatePlayersCounter(LobbyPlayersCounter.numOfLoadedPlayers);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.customNetworkManager.isServerBusy = false;
    }

    [ClientRpc]
    void RpcUpdatePlayersCounter( int numOfLoadedPlayers )
    {
        LobbyPlayersCounter.numOfLoadedPlayers = numOfLoadedPlayers;
        LobbyPlayersCounter.UpdatePlayersCounter(readyPlayers, connectedPlayers);
    }

    [Server]
    internal static IEnumerator ServerActivateLoadHeaders()
    {
        if (CustomNetworkManager.customNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.customNetworkManager.isServerBusy);
        CustomNetworkManager.customNetworkManager.isServerBusy = true;

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
        CustomNetworkManager.customNetworkManager.isServerBusy = false;
    }

    [ClientRpc]
    void RpcInitGetPlayerNames()
    {
        //Debug.Log("RpcInitGetPlayerNames()");
        playerNames.Clear();
    }

    [ClientRpc]
    void RpcGetPlayerName( string name )
    {
        //Debug.Log("RpcGetPlayerName().name - " + name);
        playerNames.Add(name);
    }

    [ClientRpc]
    void RpcUpdatePlayersNamesInHeaders()
    {
        int limit = LobbyPlayersCounter.numOfLoadedPlayers;

        //Debug.Log("RpcUpdatePlayersNamesInHeaders().limit - " + limit);

        for (int i = 0; i < limit; i++)
        {
            PILLoadHeaders[i].Initialize(playerNames[i]);
        }
    }

    [Server]
    internal static IEnumerator ServerDeactivateLoadHeaders()
    {
        if (CustomNetworkManager.customNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.customNetworkManager.isServerBusy);
        CustomNetworkManager.customNetworkManager.isServerBusy = true;

        lobbyManager.RpcDeactivateLoadHeaders();

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.customNetworkManager.isServerBusy = false;
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
        {
            pILLoadHeader.gameObject.SetActive(false);
        }
    }
}