#pragma warning disable CS0618 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BuyoutGoalCard : Effect
{
    [SerializeField] internal int buyoutPrice = 0;

    [Server]
    internal override IEnumerator EffectOnUse()
    {
        yield return base.EffectOnUse();

        if (CustomNetManager.singleton.isServerBusy)
            yield return new WaitUntil(() => !CustomNetManager.singleton.isServerBusy);
        CustomNetManager.singleton.isServerBusy = true;

        PlayerInGame player;
        player = ClientScene.FindLocalObject(targetNetId).GetComponent<PlayerInGame>();

        yield return new WaitForEndOfFrame();
        RpcAddBuyoutGoal();

        yield return new WaitForEndOfFrame();
        player.RpcDiscardCard(this.netId);

        yield return new WaitForEndOfFrame();
        CustomNetManager.singleton.isServerBusy = false;
    }

    [ClientRpc]
    protected void RpcAddBuyoutGoal()
    {
        gameManager.buyoutGoals.Add(this.gameObject);
        BuyoutGoals.AddGoalToList(this.gameObject);
    }

    [ClientRpc]
    protected void RpcRemoveBuyoutGoal()
    {
        gameManager.buyoutGoals.Remove(this.gameObject);
        StartCoroutine(BuyoutGoals.RemoveGoalFromList(this.gameObject));
    }

    [Server]
    virtual internal IEnumerator ServerOnBuyout()
    {
        if (CustomNetManager.singleton.isServerBusy)
            yield return new WaitUntil(() => !CustomNetManager.singleton.isServerBusy);
        CustomNetManager.singleton.isServerBusy = true;

        RpcRemoveBuyoutGoal();

        yield return new WaitForEndOfFrame();
        CustomNetManager.singleton.isServerBusy = false;
    }
}
