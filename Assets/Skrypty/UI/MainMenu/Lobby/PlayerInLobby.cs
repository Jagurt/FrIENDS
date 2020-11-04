#pragma warning disable CS0618 // Type too old lul

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Class for PlayerInLobby objects.
/// Handling player readiness and Toggle object.
/// </summary>
public class PlayerInLobby : NetworkBehaviour
{
    private LobbyManager lobby;
    internal PlayerManager playerManager;

    [SerializeField] [SyncVar] private bool readyToStart = false;
    [SerializeField] private Toggle toggle;
    TextMeshProUGUI nickName;

    private void Awake()
    {
        toggle = GetComponentInChildren<Toggle>();
        toggle.interactable = false;
        toggle.onValueChanged.AddListener(delegate { ToggleReady(); });

        nickName = transform.Find("PlayersName").GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;

        transform.SetParent(MainMenu.lobbyPanel);
    }

    internal void Initialize( string nickName, PlayerManager playerManager )
    {
        if (!this.nickName)
            this.nickName = GetComponentInChildren<TextMeshProUGUI>();

        this.nickName.text = nickName;
        this.playerManager = playerManager;

        // Each Spawned PlayerInLobby object calls following coroutine setting its correct positioning and color
        if (GlobalVariables.IsHost) 
            StartCoroutine(ServerUpdateAllPILPositions());
    }

    public override void OnStartAuthority()
    {
        toggle = GetComponentInChildren<Toggle>();
        toggle.interactable = true;
    }

    public void ToggleReady()
    {
        if (!hasAuthority)
            return;
        else
            CmdSwitchReadiness(toggle.isOn);
    }

    [Command]
    private void CmdSwitchReadiness( bool isOn )
    {
        StartCoroutine(ServerSwitchReadindess(isOn));        
    }
    [Server]
    IEnumerator ServerSwitchReadindess(bool isOn)
    {
        if (CustomNetworkManager.customNetworkManager.isServerBusy)
            yield return new WaitUntil(() => CustomNetworkManager.customNetworkManager.isServerBusy);
        CustomNetworkManager.customNetworkManager.isServerBusy = true;

        readyToStart = !readyToStart;
        if (readyToStart)
            LobbyManager.ReadyPlayer();
        else
            LobbyManager.UnreadyPlayer();

        yield return new WaitForEndOfFrame();
        LobbyManager.lobbyManager.RpcUpdatePlayersCounter(LobbyPlayersCounter.numOfLoadedPlayers);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.customNetworkManager.isServerBusy = false;
    }

    [Client]
    void ClientSwitchReadiness(bool isOn)
    {
        toggle.isOn = isOn;
    }

    [Server]
    static internal IEnumerator ServerUpdateAllPILPositions()
    {
        int playerCount = 0;
        int limit = LobbyManager.lobbyManager.connectedPlayers;

        for (int i = 0; i < MainMenu.lobbyPanel.childCount && playerCount < limit; i++)
        {
            PlayerInLobby player = MainMenu.lobbyPanel.GetChild(i).GetComponent<PlayerInLobby>();
            if (player != null)
            {
                if (CustomNetworkManager.customNetworkManager.isServerBusy)
                    yield return new WaitUntil(() => CustomNetworkManager.customNetworkManager.isServerBusy);
                CustomNetworkManager.customNetworkManager.isServerBusy = true;

                player.playerManager.playerIndex = playerCount;
                player.RpcUpdatePILSiblingIndex(playerCount);

                yield return new WaitForEndOfFrame();
                CustomNetworkManager.customNetworkManager.isServerBusy = false;

                playerCount++;
            }
        }
    }

    [ClientRpc]
    internal void RpcUpdatePILSiblingIndex( int newSiblingIndex )
    {
        ClientUpdatePILSiblingIndex(newSiblingIndex);
    }

    [Client]
    internal void ClientUpdatePILSiblingIndex( int newSiblingIndex )
    {
        //Debug.Log("newSiblingIndex - " + newSiblingIndex);
        transform.SetSiblingIndex(newSiblingIndex * 2 + 1);
        GetComponent<Image>().color = LobbyManager.Colors[newSiblingIndex];
    }
}
