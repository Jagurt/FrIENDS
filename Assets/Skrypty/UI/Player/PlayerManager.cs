#pragma warning disable CS0618 // Type too old lul

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerInLobby;
    [SerializeField] private GameObject playerInGame;
    CustomNetworkManager CustomNetworkManager;
    [SerializeField] [SyncVar] string nickName;

    private void Start()
    {
        DontDestroyOnLoad(this); // Setting this objec to not be destroyed on transition betwen scenes
        CustomNetworkManager = CustomNetworkManager.customNetworkManager;

        if (hasAuthority)
            CmdSpawnPlayerLobbyPanel(GlobalVariables.NickName);

        SceneManager.sceneLoaded += InitializeInGameScene;
    }

    void InitializeInGameScene( Scene scene, LoadSceneMode mode ) // Spawning PlayerInGame object in Game Scene
    {
        if (scene.name != "GameScene")
        {
            Debug.LogError("Initialization of Players In Game object outside Game Scene!");
            return;
        }

        SpawnPlayerInGame();
    }

    [Server]
    private void SpawnPlayerInGame()
    {
        if (connectionToClient.isReady && !CustomNetworkManager.isServerBusy) // If client is connected and ready, this is set by Unity automatically
        {
            playerInGame = Instantiate(playerInGame); // Creating PlayerInGame object from PlayerInGame Prefab stored in playerInGame variable and storing created object in that variable
            NetworkServer.SpawnWithClientAuthority(playerInGame, connectionToClient); // "Spawning" object - Creating object on server and all clients, assigning player who requested spawn with authority for this object.
        }
        else
            StartCoroutine(WaitForReady());
    }

    [Server]
    private IEnumerator WaitForReady()
    {
        while (!connectionToClient.isReady || CustomNetworkManager.isServerBusy)
        {
            yield return new WaitForSeconds(0.25f);
        }
        CustomNetworkManager.isServerBusy = true;

        playerInGame = Instantiate(playerInGame);
        NetworkServer.SpawnWithClientAuthority(playerInGame, connectionToClient);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.isServerBusy = false;
    }

    [Command]
    void CmdSpawnPlayerLobbyPanel( string nickName )
    {
        this.nickName = nickName;
        LobbyManager.ConnectedPlayersUpdate();
        SpawnPlayerLobbyPanel();
    }

    [Server]
    private void SpawnPlayerLobbyPanel()
    {
        playerInLobby = Instantiate(playerInLobby);
        PlayerInLobby playerLobbyPanelScript = playerInLobby.GetComponent<PlayerInLobby>();

        NetworkServer.SpawnWithClientAuthority(playerInLobby, connectionToClient);
        StartCoroutine(SyncPlayersInLobby());
    }

    [Server]
    internal static IEnumerator SyncPlayersInLobby()
    {
        foreach (var player in FindObjectsOfType<PlayerManager>())
        {
            yield return new WaitUntil(() => !CustomNetworkManager.customNetworkManager.isServerBusy);
            CustomNetworkManager.customNetworkManager.isServerBusy = true;

            player.RpcInitializePlayerLobbyPanel(player.playerInLobby.GetComponent<PlayerInLobby>().netId);

            yield return new WaitForEndOfFrame();
            CustomNetworkManager.customNetworkManager.isServerBusy = false;
        }
    }

    [ClientRpc]
    void RpcInitializePlayerLobbyPanel( NetworkInstanceId playerInLobbyNetId )
    {
        playerInLobby = ClientScene.FindLocalObject(playerInLobbyNetId);
        playerInLobby.GetComponent<PlayerInLobby>().Initialize(nickName);
    }

    [Server]
    public override void OnNetworkDestroy()
    {
        LobbyManager.ConnectedPlayersDown();
    }
}
