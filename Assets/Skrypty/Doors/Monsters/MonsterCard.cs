﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MonsterCard : Door
{
    [SerializeField] internal List<GameObject> appliedBuffs = new List<GameObject>();

    private void Start()
    {
        serverGameManager = ServerGameManager.serverGameManager;
    }

    internal override void UseCard()
    {
        PlayerInGame localPlayer = PlayerInGame.localPlayerInGame;

        if (serverGameManager.turnPhase != TurnPhase.Search || serverGameManager.fightInProggres || !localPlayer.hasTurn || serverGameManager.foughtInThisRound)
        {
            Debug.Log("Cannot fight right now!");
            GetComponent<Draggable>().ReturnToParent(); // Returning card to its former parent
            return;
        }

        localPlayer.UseCard(this.netId);
    }

    [Server]
    internal override IEnumerator EffectOnUse( NetworkInstanceId targetNetId )
    {
        PlayerInGame fightingPlayer = serverGameManager.playersObjects[serverGameManager.activePlayerIndex].GetComponent<PlayerInGame>();
        //Debug.Log("Monster in fight: " + this.gameObject);

        serverGameManager.fightInProggres = true;
        serverGameManager.fightingPlayerNetId = fightingPlayer.netId;
        serverGameManager.UpdateFightingPlayersLevel();
        serverGameManager.SetMonsterLevelTo(((MonsterValue)cardValues).level);
        serverGameManager.canPlayersTrade = false;
        serverGameManager.readyPlayers = 0;

        if (CustomNetworkManager.customNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.customNetworkManager.isServerBusy);
        CustomNetworkManager.customNetworkManager.isServerBusy = true;

        RpcInitiateFight();
    }

    [ClientRpc]
    void RpcInitiateFight()
    {
        Debug.Log("RpcInititating fight with: " + this.gameObject);
        transform.SetParent(PlayerInGame.localPlayerInGame.table);
        serverGameManager.fightingMonsters.Add(this.gameObject);
        PlayerInGame.localPlayerInGame.ProgressButton.ActivateButton();
        PlayerInGame.localPlayerInGame.HelpButton.ActivateButton();
        PlayerInGame.localPlayerInGame.levelCounter.StartFight();
        PlayerInGame.localPlayerInGame.DisableTable();
        InfoPanel.Alert("Fight with " + cardValues.name + " starts!");
    }

    virtual internal void DefeatEffect(NetworkInstanceId defeatedPlayer)
    {
        FightEndEffect();
    }

    virtual internal void TriumphEffect( NetworkInstanceId triumphantPlayer )
    {
        FightEndEffect();
    }

    virtual internal void FightEndEffect()
    {
        throw new NotImplementedException();
    }
}
