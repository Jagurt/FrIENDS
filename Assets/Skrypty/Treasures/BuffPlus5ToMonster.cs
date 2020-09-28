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

    [Server]
    internal override IEnumerator EffectOnUse( NetworkInstanceId targetNetId )
    {
        if (CustomNetworkManager.customNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.customNetworkManager.isServerBusy);
        CustomNetworkManager.customNetworkManager.isServerBusy = true;

        Debug.Log("Buffing Monster with +5 to level!");
        serverGameManager.fightingMonstersLevel += 5;

        // TODO: Inform Players about Effect

        RpcApplyBuff(targetNetId);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.customNetworkManager.isServerBusy = false;
    }

    [ClientRpc]
    internal void RpcApplyBuff( NetworkInstanceId monsterNetId)
    {
        var monster = ClientScene.FindLocalObject(monsterNetId); // Finding Monsters and Buffs Objects
        monster.GetComponent<MonsterCard>().appliedBuffs.Add(gameObject); // Adding buff do Monsters Applied Buffs List
        transform.SetParent(PlayerInGame.localPlayerInGame.table);    // Putting Buff Card on Table
        PlayerInGame.localPlayerInGame.levelCounter.UpdateLevels();
    }

    internal override void DispellEffect()
    {
        Debug.Log("Buffing Monster with +5 to level!");
        serverGameManager.fightingMonstersLevel -= 5;

        // TODO: Inform Players about Effect
    }
}
