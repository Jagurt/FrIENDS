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
    [SerializeField] [SyncVar] private int connectedPlayers = 0;
    public int ConnectedPlayers { get => connectedPlayers; }

    [SerializeField] GameObject PlayerCounter;

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


    private void Start()
    {
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

    internal static void UpdatePlayersCounter()
    {
        LobbyPlayersCounter.UpdatePlayersCounter(lobbyManager.readyPlayers, lobbyManager.connectedPlayers);
    }

    [Server]
    internal static IEnumerator ServerActivateLoadHeaders()
    {
        if (CustomNetworkManager.customNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.customNetworkManager.isServerBusy);
        CustomNetworkManager.customNetworkManager.isServerBusy = true;

        lobbyManager.RpcActivateLoadHeaders();

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.customNetworkManager.isServerBusy = false;
    }

    [ClientRpc]
    void RpcActivateLoadHeaders()
    {
        ClientActivateLoadHeaders();
    }

    [Client]
    void ClientActivateLoadHeaders()
    {
        Debug.Log("ClientActivateLoadHeaders");

        int limit = LobbyPlayersCounter.numOfLoadedPlayers;

        for (int i = 0; i < limit; i++)
        {
            PILLoadHeader pIL = MainMenu.lobbyPanel.GetChild(i).GetComponent<PILLoadHeader>();

            if (pIL)
            {
                pIL.gameObject.SetActive(true);
            }
            else
                limit += 1;
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