#pragma warning disable CS0618 // Type too old lul

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class PlayerManager : NetworkBehaviour
{
    internal static PlayerManager localPlayerManager;

    [SerializeField] private GameObject playerInLobby;
    [SerializeField] private GameObject playerInGame;
    CustomNetworkManager CustomNetworkManager;
    [SerializeField] [SyncVar] string nickName;

    public GameObject PlayerInGame { get => playerInGame; }

    [SerializeField] internal int playerIndex = 0;

    private void Start()
    {
        DontDestroyOnLoad(this); // Setting this objec to not be destroyed on transition betwen scenes
        CustomNetworkManager = CustomNetworkManager.customNetworkManager;

        if (hasAuthority)
        {
            localPlayerManager = this;
            CmdSpawnPlayerInLobby(GlobalVariables.NickName);
        }

        if (GlobalVariables.IsHost)
        {
            Debug.Log("connectionToClient.address - " + connectionToClient.address);
        }
    }

    [Server]
    internal static void ServerPrepToStartGame()
    {
        PlayerManager[] playerManagers = FindObjectsOfType<PlayerManager>();

        foreach (var playerManager in playerManagers)
        {
            CustomNetworkConnection conn = CustomNetworkManager.playingConnections
                .Find(x => x.address.Equals(playerManager.connectionToClient.address));

            if (conn != null)
                conn.clientOwnedObjects.Add(playerManager.netId);

            SceneManager.sceneLoaded += playerManager.InitializeInGameScene;
        }
    }

    void InitializeInGameScene( Scene scene, LoadSceneMode mode ) // Spawning PlayerInGame object in Game Scene
    {
        if (scene.name != "GameScene")
        {
            return;
        }

        StartCoroutine(ServerWaitToSpawnPlayerInGame());
    }

    [Server]
    private IEnumerator ServerWaitToSpawnPlayerInGame()
    {
        while (!connectionToClient.isReady || CustomNetworkManager.isServerBusy) // "!connectionToClient.isReady" Wait if client isn't connected and ready, this is set by Unity automatically
        {
            yield return new WaitForSeconds(0.1f);
        }
        CustomNetworkManager.isServerBusy = true;

        playerInGame = Instantiate(playerInGame);                                   // Creating PlayerInGame object from PlayerInGame Prefab stored in playerInGame variable and storing created object in that variable
        NetworkServer.SpawnWithClientAuthority(playerInGame, connectionToClient);   // "Spawning" object - Creating object on server and all clients, assigning player who requested spawn with authority for this object.
        CustomNetworkManager.playingConnections
            .Find(x => x.address.Equals(connectionToClient.address))
            .clientOwnedObjects.Add(playerInGame.GetComponent<PlayerInGame>().netId);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }

    [Command]
    void CmdSpawnPlayerInLobby( string nickName )
    {
        if (SceneManager.GetActiveScene().name.Equals("GameScene"))
            return;

        this.nickName = nickName;
        LobbyManager.ConnectedPlayersUpdate();
        StartCoroutine(SpawnPlayerInLobby(nickName));
    }

    [Server]
    IEnumerator SpawnPlayerInLobby( string nickName )
    {
        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        playerInLobby = Instantiate(playerInLobby);

        NetworkServer.SpawnWithClientAuthority(playerInLobby, connectionToClient);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;

        StartCoroutine(SyncPlayersInLobby());
    }

    [Server]
    internal static IEnumerator SyncPlayersInLobby()
    {
        foreach (var player in FindObjectsOfType<PlayerManager>())
        {
            yield return new WaitUntil(() => !CustomNetworkManager.customNetworkManager.isServerBusy);
            CustomNetworkManager.customNetworkManager.isServerBusy = true;

            player.RpcInitializePlayerInLobby(player.playerInLobby.GetComponent<PlayerInLobby>().netId);

            yield return new WaitForEndOfFrame();
            CustomNetworkManager.customNetworkManager.isServerBusy = false;
        }
    }

    [ClientRpc]
    void RpcInitializePlayerInLobby( NetworkInstanceId playerInLobbyNetId )
    {
        playerInLobby = ClientScene.FindLocalObject(playerInLobbyNetId);
        playerInLobby.GetComponent<PlayerInLobby>().Initialize(nickName, this);
    }

    [Server]
    public override void OnNetworkDestroy() // According to OnServerDisconnect() override, this should be called only in "TitleScene", more specifically in Lobby
    {
        LobbyManager.ConnectedPlayersDown();
        CustomNetworkManager.playingConnections.Remove(
            CustomNetworkManager.playingConnections
            .Find(x => x.address.Equals(connectionToClient.address)
            ));
    }
}
