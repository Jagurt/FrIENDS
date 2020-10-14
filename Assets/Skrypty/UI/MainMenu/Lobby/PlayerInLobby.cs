﻿#pragma warning disable CS0618 // Type too old lul

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerInLobby : NetworkBehaviour
{
    private LobbyManager lobby;

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

        StartCoroutine(PlayerManager.playerManager.ServerUpdatePosition());
    }

    internal void Initialize( string nickName )
    {
        if (!this.nickName)
            this.nickName = GetComponentInChildren<TextMeshProUGUI>();

        this.nickName.text = nickName;
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
        readyToStart = !readyToStart;
        if (readyToStart)
            LobbyManager.ReadyPlayer();
        else
            LobbyManager.UnreadyPlayer();

        // TODO: Update Readiness Icon

        RpcSwitchReadiness(isOn);
    }

    [ClientRpc]
    private void RpcSwitchReadiness( bool isOn )
    {
        toggle.isOn = isOn;
        LobbyManager.UpdatePlayersCounter();
    }

    // (siblingIndex - (siblingIndex % 2)) / 2
    [Client]
    static internal void ClientUpdateAllPositions()
    {
        int playerCount = 0;
        int limit = LobbyManager.lobbyManager.ConnectedPlayers;

        for (int i = 0; i < MainMenu.lobbyPanel.childCount && playerCount < limit; i++)
        {
            PlayerInLobby player = MainMenu.lobbyPanel.GetChild(i).GetComponent<PlayerInLobby>();
            if (player != null)
            {
                Debug.Log("playerCount - " + playerCount);
                player.GetComponent<Image>().color = LobbyManager.Colors[playerCount];
                player.transform.SetSiblingIndex(playerCount * 2 + 1);
                playerCount++;
            }
        }
    }
}
