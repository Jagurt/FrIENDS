#pragma warning disable CS0618 // Type too old lul

using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] private StartGameButtonInLobby startGameButton;
    [SerializeField] private Transform lobbyPanel;
    [SerializeField] [SyncVar] private int readyPlayers = 0;
    [SerializeField] [SyncVar] private int connectedPlayers = 0;

    public Transform LobbyPanel { get => lobbyPanel; }

    public void ReadyPlayer()
    {
        readyPlayers += 1;
        ReadyCheck();
    }

    public void UnreadyPlayer()
    {
        readyPlayers -= 1;
        ReadyCheck();
    }

    public void ConnectedPlayersUpdate()
    {
        connectedPlayers = NetworkServer.connections.Count;
        ReadyCheck();
    }

    public void ConnectedPlayersDown()
    {
        connectedPlayers -= 1;
        ReadyCheck();
    }

    void ReadyCheck()
    {
        if (readyPlayers == connectedPlayers) startGameButton.EnableStartGameButton();
        else startGameButton.DisableStartGameButton();
    }
}   