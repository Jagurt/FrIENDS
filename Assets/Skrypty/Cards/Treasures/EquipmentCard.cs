using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EquipmentCard : TreasureCard
{
    void Start()
    {
        Initialize();
    }

    internal override void UseCard()
    {
        PlayerInGame.localPlayerInGame.UseCardOnLocalPlayer(this.netId);
    }

    [Server]
    internal override IEnumerator EffectOnUse( NetworkInstanceId targetNetId )
    {
        PlayerInGame player = ClientScene.FindLocalObject(targetNetId).GetComponent<PlayerInGame>(); // Finding player locally via theirs "NetworkInstanceId"
        short levelChange = 0;

        if (CustomNetworkManager.customNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.customNetworkManager.isServerBusy);
        CustomNetworkManager.customNetworkManager.isServerBusy = true;

        levelChange += cardValues.level;  // Modifying players level according to cards level variable

        EqItemSlot eqSlot = player.equipment.GetChild((int)(cardValues as EquipmentValue).eqPart).GetComponentInChildren<EqItemSlot>();

        if (eqSlot.heldItem != null)
        {
            levelChange -= eqSlot.heldItem.GetComponent<EquipmentCard>().cardValues.level;
            player.Level += levelChange;

            yield return new WaitForEndOfFrame();

            RpcSwitchEq(targetNetId);
        }
        else
        {
            player.Level += levelChange;

            yield return new WaitForEndOfFrame();

            RpcEquip(targetNetId);
        }

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.customNetworkManager.isServerBusy = false;
    }

    [ClientRpc]
    void RpcEquip( NetworkInstanceId targetNetId )
    {
        PlayerInGame player = ClientScene.FindLocalObject(targetNetId).GetComponent<PlayerInGame>();    // Finding player locally via its "NetworkInstanceId"
        player.LocalEquip(this.gameObject);
        serverGameManager.StoredCardUsesToConfirm.Remove(gameObject);
    }

    [ClientRpc]
    void RpcSwitchEq( NetworkInstanceId targetNetId )
    {
        PlayerInGame player = ClientScene.FindLocalObject(targetNetId).GetComponent<PlayerInGame>();    // Finding player locally via its "NetworkInstanceId"
        player.LocalSwitchEq(this.netId);
    }
}
