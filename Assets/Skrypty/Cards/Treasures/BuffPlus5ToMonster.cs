#pragma warning disable CS0618 // Type too old lul

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BuffPlus5ToMonster : BuffCard
{
    void Start()
    {
        entityToApplyOn = Entity.Monster;
        serverGameManager = ServerGameManager.serverGameManager;
        Initialize();
    }

    /// <summary>
    /// Buffing monster with +5 to level.
    /// </summary>
    [Server]
    internal override IEnumerator EffectOnUse( )
    {
        if (CustomNetworkManager.customNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.customNetworkManager.isServerBusy);
        CustomNetworkManager.customNetworkManager.isServerBusy = true;

        Debug.Log("Buffing Monster with +5 to level!");
        serverGameManager.fightingMonstersLevel += 5;

        RpcApplyBuff(targetNetId);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.customNetworkManager.isServerBusy = false;
    }
    /// <summary>
    /// Applying buff on clients.
    /// </summary>
    [ClientRpc]
    internal void RpcApplyBuff( NetworkInstanceId monsterNetId)
    {
        var monster = ClientScene.FindLocalObject(monsterNetId); // Finding Monsters and Buffs Objects
        monster.GetComponent<MonsterCard>().appliedBuffs.Add(gameObject); // Adding buff do Monsters Applied Buffs List
        transform.SetParent(TableDropZone.tableDropZone.transform);    // Putting Buff Card on Table
        LevelCounter.UpdateLevels();
    }
    /// <summary> Called when removing buff due to some cards effect. </summary>
    internal override void DispellEffect()
    {
        Debug.Log("Buffing Monster with +5 to level!");
        serverGameManager.fightingMonstersLevel -= 5;

        // TODO: Inform Players about Effect
    }
}
