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
    CustomNetworkManager CustomNetworkManager;

    [SerializeField] private GameObject playerInLobby;
    [SerializeField] private GameObject playerInGame;
    [SerializeField] internal string address;
    [SerializeField] [SyncVar] string nickName;

    public GameObject PlayerInGame { get => playerInGame; }

    [SerializeField] internal int playerIndex = 0;

    private void Start()
    {
        // Setting this object to not be destroyed when changing scenes.
        DontDestroyOnLoad(this); 
        CustomNetworkManager = CustomNetworkManager.customNetworkManager;

        // If this object belongs to local player.
        if (hasAuthority)
        {
            localPlayerManager = this;
            // PlayerInLobby should only be created if player enters game which is in TitleScene
            if (SceneManager.GetActiveScene().name.Equals("TitleScene"))
                CmdSpawnPlayerInLobby(GlobalVariables.NickName);
        }

        if (GlobalVariables.IsHost)
        {
            address = connectionToClient.address;

            // If this object is created, Start() is called on host(server), and Scene Name is "GameScene"
            // I know that this is object owned by player that has reconnected.
            // In that case I need to reassign correc PlayerInGame object to that reconnected player. 
            if (SceneManager.GetActiveScene().name.Equals("GameScene"))
                StartCoroutine(ServerWaitToAssignPlayerInGame());
        }
    }
    /// <summary>
    /// Called when Start Game Button in Lobby is pressed.
    /// Finds all players that are connected to lobby and assignes their main object paired with their address.
    /// Prepares for spawning PlayerInGame object for this player after changing scene.
    /// </summary>
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

            SceneManager.sceneLoaded += playerManager.ServerInitializeInGameScene;
        }
    }
    /// <summary>
    /// Delegate added to SceneManager.sceneLoaded event to spawn PlayerInGame object properly.
    /// </summary>
    [Server]
    void ServerInitializeInGameScene( Scene scene, LoadSceneMode mode )
    {
        if (scene.name != "GameScene")
            return;
        StartCoroutine(ServerWaitToSpawnPlayerInGame());
        // I need to substract this delegate from event SceneManager.sceneLoaded, becouse it would run additional times 
        // any time we enter GameScene, e.g. upon reconnection.
        SceneManager.sceneLoaded -= this.ServerInitializeInGameScene;
    }
    /// <summary>
    /// Coroutine that waits for player to be ready to spawn his PlayerInGame object.
    /// Creates PlayerInGame object and Spawns ( tells all clients to create it ) with authority assigned to correct player.
    /// Lastly adds this object to correct players CustomNetworkConnection.clientOwnedObjects list.
    /// </summary>
    [Server]
    private IEnumerator ServerWaitToSpawnPlayerInGame()
    {
        while (!connectionToClient.isReady || CustomNetworkManager.isServerBusy) // "!connectionToClient.isReady" Wait if client isn't connected and ready, this is set by Unity automatically
        {
            Debug.Log("ServerWaitToSpawnPlayerInGame");
            yield return new WaitForSeconds(0.1f);
        }
        CustomNetworkManager.isServerBusy = true;

        playerInGame = Instantiate(playerInGame);                                   // Creating PlayerInGame object from PlayerInGame Prefab stored in playerInGame variable and storing created object in that variable
        NetworkServer.SpawnWithClientAuthority(playerInGame, connectionToClient);   // "Spawning" object - Creating object on server and all clients, assigning authority over this object to correct player.
        CustomNetworkManager.playingConnections
            .Find(x => x.address.Equals(connectionToClient.address))
            .clientOwnedObjects.Add(playerInGame.GetComponent<PlayerInGame>().netId);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }
    /// <summary>
    /// Waits to player to be ready, reassigns its objects to them and synchronizes objects in hierarchy.
    /// </summary>
    [Server]
    private IEnumerator ServerWaitToAssignPlayerInGame()
    {
        while (!connectionToClient.isReady || CustomNetworkManager.isServerBusy) // "!connectionToClient.isReady" Wait if client isn't connected and ready, this is set by Unity automatically
        {
            Debug.Log("ServerWaitToAssignPlayerInGame");
            yield return new WaitForSeconds(0.1f);
        }
        CustomNetworkManager.isServerBusy = true;

        // In CustomNetworkManager.playingConnections ( -  list of connections that are saved in lobby )
        // .Find(x => x.address == connectionToClient.address  ( - find object in list whose ip address == this objects owner ip adress )
        // x.clientOwnedObjects[0] == NetworkInstanceId.Invalid); ( - and whose main objects (PlayerManager) netId is invalid, so we know that he has disconnected )
        var playingConn = CustomNetworkManager.playingConnections
                    .Find(x => x.address == connectionToClient.address &&
                    x.clientOwnedObjects[0] == NetworkInstanceId.Invalid);      // This is set in CustomNetworkManager.OnServerDisconnect

        // Since PlayerManager(this) object should never be created in "GameScene" for nothing else but reconnected players
        // playingConn should never be null.
        Debug.Log("FoundPlayingConn - " + playingConn);

        // Since we found player connection of reconnected player (owner of this object)
        // We should store its new main objects netId.
        playingConn.clientOwnedObjects[0] = this.netId;
        GameObject GO = ClientScene.FindLocalObject(playingConn.clientOwnedObjects[1]);
        // Reassign authority over PlayerInGame object to reconnected player.
        bool authorityAssigned = GO.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
        Debug.Log("authorityAssigned - " + authorityAssigned + " to GameObject - " + GO);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;

        // Synchronize scene hierarchy.
        foreach (var player in FindObjectsOfType<PlayerInGame>())
        {
            Debug.Log("ServerWaitToAssignPlayerInGame(): player.handContent - " + player.handContent);
            player.ServerReconnect();
        }
    }
    /// <summary>
    /// Called in Start() by the client to command server to spawn their PlayerInLobby object.
    /// </summary>
    /// <param name="nickName"> NickName of connected player </param>
    [Command]
    void CmdSpawnPlayerInLobby( string nickName )
    {
        if (SceneManager.GetActiveScene().name.Equals("GameScene"))
            return;

        this.nickName = nickName;
        LobbyManager.UpdateConnectedPlayers();
        StartCoroutine(SpawnPlayerInLobby(nickName));
    }
    /// <summary>
    /// Coroutine that creates clients PlayerInLobby object on server
    /// and then Spawns it ( tells all clients to create and synchronize it ) with correct authority assigned.
    /// </summary>
    /// <param name="nickName"> NickName of connected player </param>
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
    /// <summary>
    /// Synchronizes names of players on PlayerInLobby objects.
    /// </summary>
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
    /// <summary>
    /// Called when this object is destroyed by the network.
    /// </summary>
    [Server]
    public override void OnNetworkDestroy()
    {
        LobbyManager.ConnectedPlayersDown();
    }
}
