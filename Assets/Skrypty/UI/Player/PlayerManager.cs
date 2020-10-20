#pragma warning disable CS0618 // Type too old lul

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class PlayerManager : NetworkBehaviour
{
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
            CmdSpawnPlayerInLobby(GlobalVariables.NickName);

        //SceneManager.sceneLoaded += InitializeInGameScene;
    }

    [Server]
    internal static void ServerPrepToStartGame()
    {
        PlayerManager[] playerManagers = FindObjectsOfType<PlayerManager>();

        foreach (var playerManager in playerManagers)
        {
            SceneManager.sceneLoaded += playerManager.InitializeInGameScene;
        }
    }

    void InitializeInGameScene( Scene scene, LoadSceneMode mode ) // Spawning PlayerInGame object in Game Scene
    {
        if (scene.name != "GameScene")
        {
            Debug.LogError("Initialization of Players In Game object outside Game Scene!");
            return;
        }

        ServerSpawnPlayerInGame();
    }

    [Server]
    private void ServerSpawnPlayerInGame()
    {
        if (connectionToClient.isReady && !CustomNetworkManager.isServerBusy) // If client is connected and ready, this is set by Unity automatically
        {
            playerInGame = Instantiate(playerInGame); // Creating PlayerInGame object from PlayerInGame Prefab stored in playerInGame variable and storing created object in that variable
            NetworkServer.SpawnWithClientAuthority(playerInGame, connectionToClient); // "Spawning" object - Creating object on server and all clients, assigning player who requested spawn with authority for this object.
        }
        else
            StartCoroutine(WaitToSpawnPlayerInGame());
    }

    [Server]
    private IEnumerator WaitToSpawnPlayerInGame()
    {
        while (!connectionToClient.isReady || CustomNetworkManager.isServerBusy)
        {
            yield return new WaitForSeconds(0.1f);
        }
        CustomNetworkManager.isServerBusy = true;

        playerInGame = Instantiate(playerInGame);
        NetworkServer.SpawnWithClientAuthority(playerInGame, connectionToClient);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }

    [Command]
    void CmdSpawnPlayerInLobby(string nickName)
    {
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
    public override void OnNetworkDestroy()
    {
        LobbyManager.ConnectedPlayersDown();
    }
}
