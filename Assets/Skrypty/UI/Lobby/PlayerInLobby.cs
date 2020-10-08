#pragma warning disable CS0618 // Type too old lul

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
        nickName = transform.Find("PlayersName").GetComponent<TextMeshProUGUI>();
        toggle.interactable = false;
    }

    private void Start()
    {
        lobby = FindObjectOfType<LobbyManager>();
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;

        LobbyManager lobbyManager = FindObjectOfType<LobbyManager>();
        if (lobbyManager && lobbyManager.LobbyPanel)
            transform.SetParent(lobbyManager.LobbyPanel);
    }

    internal void Initialize(string nickName)
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
            lobby.ReadyPlayer();
        else
            lobby.UnreadyPlayer();

        // TODO: Update Readiness Icon

        RpcSwitchReadiness(isOn);
    }

    [ClientRpc]
    private void RpcSwitchReadiness( bool isOn )
    {
        toggle.isOn = isOn;
    }
}
