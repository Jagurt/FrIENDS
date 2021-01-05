#pragma warning disable CS0618 // Type too old lul

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EquipmentCard : TreasureCard
{
    GameObject equipmentSlotInfoGO;
    GameObject eqLevelInfoGO;

    protected override IEnumerator ClientWaitInstantiateAddons()
    {
        yield return base.ClientWaitInstantiateAddons();

        equipmentSlotInfoGO = Instantiate(CardsAddons.equipmentSlotInfoPrefab, transform);
        equipmentSlotInfoGO.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = (cardValues as EquipmentValue).eqPart.ToString();
        eqLevelInfoGO = Instantiate(CardsAddons.levelInfoPrefab, transform);
        eqLevelInfoGO.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = cardValues.level.ToString();
    }

    internal override void UseCard()
    {
        PlayerInGame.localPlayerInGame.UseCardOnLocalPlayer(this.netId);
    }

    /// <summary>
    /// Equip or switch( take off equipped item and wear unequipped ) item.
    /// Modify level accordingly.
    /// </summary>
    [Server]
    internal override IEnumerator EffectOnUse()
    {
        yield return base.EffectOnUse();

        PlayerInGame player = ClientScene.FindLocalObject(targetNetId).GetComponent<PlayerInGame>(); // Finding player locally via theirs "NetworkInstanceId"

        if (CustomNetManager.singleton.isServerBusy)
            yield return new WaitUntil(() => !CustomNetManager.singleton.isServerBusy);
        CustomNetManager.singleton.isServerBusy = true;

        EqItemSlot eqSlot = player.equipment.GetChild((int)(cardValues as EquipmentValue).eqPart).GetComponentInChildren<EqItemSlot>();

        if (eqSlot.heldItem != null)
            RpcSwitchEq(targetNetId);
        else
            RpcEquip(targetNetId);

        yield return new WaitUntil(() => player.equippedItems.Contains(this.gameObject));

        yield return new WaitForEndOfFrame();
        ServerOnEquip();

        yield return new WaitForEndOfFrame();
        gameManager.ServerUpdateFightingPlayersLevel();

        CustomNetManager.singleton.isServerBusy = false;
    }

    [ClientRpc]
    void RpcEquip( NetworkInstanceId targetNetId )
    {
        PlayerInGame player = ClientScene.FindLocalObject(targetNetId).GetComponent<PlayerInGame>();    // Finding player locally via its "NetworkInstanceId"
        player.ClientEquip(this.gameObject);
        gameManager.cardsUsageQueue.Remove(gameObject);
    }

    [ClientRpc]
    void RpcSwitchEq( NetworkInstanceId targetNetId )
    {
        PlayerInGame player = ClientScene.FindLocalObject(targetNetId).GetComponent<PlayerInGame>();    // Finding player locally via its "NetworkInstanceId"
        player.ClientSwitchEq(this.netId);
    }

    [Server]
    virtual internal void ServerOnEquip()
    {
        foreach (var monster in gameManager.fightingMonsters)
            monster.GetComponent<MonsterCard>().ServerEquipmentCheck();
    }
}
