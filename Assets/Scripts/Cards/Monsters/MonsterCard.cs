#pragma warning disable CS0618 // Type too old lul

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class MonsterCard : Card
{
    [SerializeField] internal List<GameObject> appliedEffects = new List<GameObject>();
    GameObject monsterLevelInfo;
    Transform appliedEffectsContent;

    void Start()
    {
        gameManager = GameManager.singleton;
        Initialize();
    }

    protected override IEnumerator ClientWaitInstantiateAddons()
    {
        yield return base.ClientWaitInstantiateAddons();

        // Creating monster level displaying component and updating its text.
        monsterLevelInfo = Instantiate(CardsAddons.monsterLevelInfoPrefab, transform);
        monsterLevelInfo.transform.Find("LevelInfo").Find("TextMeshPro Text").GetComponent<TextMeshProUGUI>().text = cardValues.level.ToString();
        appliedEffectsContent = monsterLevelInfo.transform
            .Find("MonsterAppliedEffects")
            .Find("BaseContentSizeFitter")
            .Find("ContentParentRect")
            .Find("Content");
    }

    /// <summary> Checking if player can start a fight. </summary>
    internal override void UseCard()
    {
        PlayerInGame localPlayer = PlayerInGame.localPlayerInGame;

        if (gameManager.turnPhase != TurnPhase.Search || gameManager.fightInProggres || !localPlayer.hasTurn || gameManager.foughtInThisRound)
        {
            InfoPanel.AlertCannotUseCard();
            // Return card to hand
            StartCoroutine(
                GetComponent<Draggable>()
                .ClientSlideWithNewPlaceholder(PlayerInGame.localPlayerInGame.handContent)
                );
            return;
        }

        localPlayer.UseCardOnLocalPlayer(this.netId);
    }

    /// <summary> Starting a fight with a monster. </summary>
    [Server]
    internal override IEnumerator EffectOnUse()
    {
        PlayerInGame fightingPlayer = gameManager.playersObjects[gameManager.activePlayerIndex].GetComponent<PlayerInGame>();
        //Debug.Log("Monster in fight: " + this.gameObject);

        if (CustomNetManager.singleton.isServerBusy)
            yield return new WaitUntil(() => !CustomNetManager.singleton.isServerBusy);
        CustomNetManager.singleton.isServerBusy = true;

        gameManager.fightInProggres = true;
        gameManager.fightingPlayerNetId = fightingPlayer.netId;
        gameManager.ServerUpdateFightingPlayersLevel();
        gameManager.fightingMonstersLevel = ((MonsterValue)cardValues).level;
        gameManager.readyPlayers = 0;

        yield return new WaitForEndOfFrame();

        RpcInitiateFight();

        yield return new WaitForEndOfFrame();
        CustomNetManager.singleton.isServerBusy = false;

        yield return ServerActivateCard();
    }

    /// <summary> Informing clients about started fight. </summary>
    [ClientRpc]
    void RpcInitiateFight()
    {
        //Debug.Log("RpcInititating fight with: " + this.gameObject);
        gameManager.fightingMonsters.Add(this.gameObject);
        ClientSetActiveCardButtons(false);
        PlayerInGame.localPlayerInGame.progressButton.ActivateButton();
        HelpButton.Activate();
        LevelCounter.OnStartFight();
        InfoPanel.Alert("Fight with " + cardValues.name + " starts!");
    }

    virtual internal void DefeatEffect( NetworkInstanceId defeatedPlayer )
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

    [Server]
    virtual internal void ServerEquipmentCheck()
    {
        gameManager.ServerUpdateMonstersLevels();
    }

    [Client]
    internal void ClientApplyEffect(GameObject effect)
    {
        appliedEffects.Add(effect);

        GameObject appliedEffect = Instantiate(CardsAddons.monsterAppliedEffectPrefab, appliedEffectsContent);
        appliedEffect.GetComponent<MonsterAppliedEffect>()
            .Initialize(effect.GetComponent<Card>().cardValues.name);
    }

    [Client]
    internal void ClientRemoveAllEffects()
    {
        appliedEffects.Clear();

        while (appliedEffectsContent.childCount > 0)
            Destroy(appliedEffectsContent.GetChild(0).gameObject);
    }

    [Client]
    override internal void ClientOnDiscard()
    {
        ClientRemoveAllEffects();
    }
}
