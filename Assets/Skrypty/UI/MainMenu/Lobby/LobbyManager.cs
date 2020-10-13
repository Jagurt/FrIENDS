#pragma warning disable CS0618 // Type too old lul

using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class LobbyManager : NetworkBehaviour
{
    static LobbyManager lobbyManager;

    [SerializeField] private StartGameButtonInLobby startGameButton;
    [SerializeField] private Transform lobbyPanel;
    [SerializeField] [SyncVar] private int readyPlayers = 0;
    [SerializeField] [SyncVar] private int connectedPlayers = 0;

    public Transform LobbyPanel { get => lobbyPanel; }
    [SerializeField] GameObject PlayerCounter;

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
        if (lobbyManager.readyPlayers == lobbyManager.connectedPlayers) lobbyManager.startGameButton.EnableStartGameButton();
        else lobbyManager.startGameButton.DisableStartGameButton();
    }

    internal static void UpdatePlayersCounter()
    {
        LobbyPlayersCounter.UpdatePlayersCounter((short)lobbyManager.readyPlayers, (short)lobbyManager.connectedPlayers);
    }
}