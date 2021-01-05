#pragma warning disable CS0618 // Type too old lul

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BuffPlus5ToMonster : Effect
{
    void Start()
    {
        target = Target.Monster;
        choosable = true;
        gameManager = GameManager.singleton;
        Initialize();
    }

    /// <summary> Buffing monster with +5 to level. </summary>
    [Server]
    internal override IEnumerator EffectOnUse()
    {
        yield return base.EffectOnUse();

        if (CustomNetManager.singleton.isServerBusy)
            yield return new WaitUntil(() => !CustomNetManager.singleton.isServerBusy);
        CustomNetManager.singleton.isServerBusy = true;

        Debug.Log("Buffing Monster with +5 to level!");
        gameManager.fightingMonstersLevel += 5;

        RpcApplyBuff(targetNetId);

        yield return new WaitForEndOfFrame();
        CustomNetManager.singleton.isServerBusy = false;
    }

    /// <summary> Applying buff on clients. </summary>
    [ClientRpc]
    internal void RpcApplyBuff( NetworkInstanceId monsterNetId)
    {
        GameObject monster = ClientScene.FindLocalObject(monsterNetId);     // Finding Monster Object
        monster.GetComponent<MonsterCard>().ClientApplyEffect(gameObject);   // Adding buff do Monsters Applied Buffs List
    }

    /// <summary> Called when removing buff due to some cards effect. </summary>
    internal override void DispellEffect()
    {
        Debug.Log("Buffing Monster with +5 to level!");
        gameManager.fightingMonstersLevel -= 5;

        // TODO: Inform Players about Effect
    }
}
