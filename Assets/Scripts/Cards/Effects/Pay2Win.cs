#pragma warning disable CS0618 // Type too old lul

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Pay2Win : BuyoutGoalCard
{
    private void Start()
    {
        target = Target.Player;
        gameManager = GameManager.singleton;
        Initialize();
    }

    internal override void UseCard()
    {
        PlayerInGame.localPlayerInGame.UseCardOnLocalPlayer(this.netId);
    }

    /// <summary> Puts effect in play which prevents players from gaining level this turn. </summary>
    [Server]
    internal override IEnumerator EffectOnUse()
    {
        yield return base.EffectOnUse();
        gameManager.turnActiveffects.Add(this.gameObject);
    }

    [Server]
    internal override IEnumerator ServerOnBuyout()
    {
        yield return base.ServerOnBuyout();
        gameManager.turnActiveffects.Remove(this.gameObject);
    }

    [Server]
    internal override IEnumerator ServerOnTurnEnd()
    {
        if (CustomNetManager.singleton.isServerBusy)
            yield return new WaitUntil(() => !CustomNetManager.singleton.isServerBusy);
        CustomNetManager.singleton.isServerBusy = true;

        RpcRemoveBuyoutGoal();

        yield return new WaitForEndOfFrame();
        CustomNetManager.singleton.isServerBusy = false;
    }
}
