﻿using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CurseLevelDown : Effect
{
    private void Start()
    {
        serverGameManager = ServerGameManager.serverGameManager;
        target = Target.Player;
        choosable = true;
        Initialize();
    }

    [Server]
    internal override IEnumerator EffectOnUse( NetworkInstanceId targetNetId )
    {
        if (CustomNetworkManager.customNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.customNetworkManager.isServerBusy);
        CustomNetworkManager.customNetworkManager.isServerBusy = true;


        PlayerInGame player = null;

        if (targetNetId != NetworkInstanceId.Invalid) // If player to curse is not chosen
            player = ClientScene.FindLocalObject(targetNetId).GetComponent<PlayerInGame>(); // Player which currently has turn is chosen, this should only ocurr when curse is drawn as turns first door.

        if (!player)
        {
            if (serverGameManager.activePlayerIndex >= 0)
                player = serverGameManager.playersObjects[serverGameManager.activePlayerIndex].GetComponent<PlayerInGame>();
            else
                player = serverGameManager.playersObjects[0].GetComponent<PlayerInGame>();
        }


        player.Level -= 1;

        PlayerInGame.localPlayerInGame.RpcDiscardCard(this.netId);
        serverGameManager.TurnOwnerReadiness();
    }
}
