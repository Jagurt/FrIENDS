#pragma warning disable CS0618 // Type too old lul

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TradeButton : MonoBehaviour
{
    Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(delegate { OnClick(); });
    }

    /// <summary> Finding which players can trade right now and starting choosing. </summary>
    private void OnClick()
    {
        List<PlayerInGame> playersFreeToTrade = new List<PlayerInGame>();

        // In all player objects.
        foreach (var player in ServerGameManager.serverGameManager.playersObjects)
        {
            NetworkInstanceId playerNetId = player.GetComponent<PlayerInGame>().netId;
            // If player is not fighting, helping with fight or isn't same player who clicked this button.
            if (playerNetId != ServerGameManager.serverGameManager.fightingPlayerNetId &&
                playerNetId != ServerGameManager.serverGameManager.helpingPlayerNetId &&
                playerNetId != PlayerInGame.localPlayerInGame.netId)

                playersFreeToTrade.Add(player.GetComponent<PlayerInGame>());
        }

        if (playersFreeToTrade.Count > 0)
            PlayerInGame.localPlayerInGame.ChoosePlayerToTradeWith(playersFreeToTrade.ToArray());
        else
            InfoPanel.Alert("Zero potential traders found!");
    }
}
