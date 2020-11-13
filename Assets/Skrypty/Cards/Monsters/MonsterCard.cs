#pragma warning disable CS0618 // Type too old lul

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MonsterCard : Card
{
    [SerializeField] internal List<GameObject> appliedBuffs = new List<GameObject>();

    private void Start()
    {
        serverGameManager = ServerGameManager.serverGameManager;
    }

    /// <summary> Checking if player can start a fight. </summary>
    internal override void UseCard()
    {
        PlayerInGame localPlayer = PlayerInGame.localPlayerInGame;

        if (serverGameManager.turnPhase != TurnPhase.Search || serverGameManager.fightInProggres || !localPlayer.hasTurn || serverGameManager.foughtInThisRound)
        {
            Debug.Log("Cannot fight right now!");
            GetComponent<Draggable>().ReturnToParent(); // Returning card to its former parent
            return;
        }

        localPlayer.UseCardOnLocalPlayer(this.netId);
    }

    /// <summary>
    /// Starting a fight with a monster.
    /// </summary>
    [Server]
    internal override IEnumerator EffectOnUse()
    {
        PlayerInGame fightingPlayer = serverGameManager.playersObjects[serverGameManager.activePlayerIndex].GetComponent<PlayerInGame>();
        //Debug.Log("Monster in fight: " + this.gameObject);

        if (CustomNetworkManager.customNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.customNetworkManager.isServerBusy);
        CustomNetworkManager.customNetworkManager.isServerBusy = true;

        serverGameManager.fightInProggres = true;
        serverGameManager.fightingPlayerNetId = fightingPlayer.netId;
        serverGameManager.ServerUpdateFightingPlayersLevel();
        serverGameManager.fightingMonstersLevel = ((MonsterValue)cardValues).level;
        serverGameManager.readyPlayers = 0;

        yield return new WaitForEndOfFrame();

        RpcInitiateFight();

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.customNetworkManager.isServerBusy = false;
    }

    /// <summary> Informing clients about started fight. </summary>
    [ClientRpc]
    void RpcInitiateFight()
    {
        //Debug.Log("RpcInititating fight with: " + this.gameObject);
        transform.SetParent(TableDropZone.tableDropZone.transform);
        serverGameManager.fightingMonsters.Add(this.gameObject);
        ClientSetActiveCardButtons(false);
        PlayerInGame.localPlayerInGame.progressButton.ActivateButton();
        HelpButton.Activate();
        LevelCounter.OnStartFight();
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

    virtual protected void FightEndEffect()
    {
        
    }
}
