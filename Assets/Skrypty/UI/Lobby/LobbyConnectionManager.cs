#pragma warning disable CS0618 // Type too old lul

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LobbyConnectionManager : NetworkBehaviour
{
    public GameObject PlayerPanel;
    public GameObject LobbyPanel;

    [Command]
    public void CmdSpawnPlayerPanel()
    {
        SpawnPlayerPanel();
    }

    [Server]
    public void SpawnPlayerPanel()
    {
        PlayerPanel = Instantiate(PlayerPanel.gameObject);
        PlayerPanel.transform.SetParent(LobbyPanel.transform);
        PlayerPanel.transform.GetComponent<PlayerInLobby>().Initialize(GlobalVariables.NickName);
        NetworkServer.Spawn(PlayerPanel);
    }
}
