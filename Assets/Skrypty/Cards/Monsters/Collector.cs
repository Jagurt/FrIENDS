#pragma warning disable CS0618 // Type too old lul

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Collector : MonsterCard
{
    void Start()
    {
        serverGameManager = ServerGameManager.serverGameManager;
        Initialize();
    }

    /// <summary>
    /// Increase power of a monster for each "Glass" object that player is wearing.
    /// </summary>
    [Server]
    override internal IEnumerator EffectOnUse( NetworkInstanceId targetNetId )
    {
        PlayerInGame fightingPlayer = ClientScene.FindLocalObject(targetNetId).GetComponent<PlayerInGame>();

        foreach (var item in fightingPlayer.equippedItems)
        {
             if(item.GetComponent<EquipmentCard>().cardValues.name.Contains("Glass"))
            {
                (cardValues as MonsterValue).treasuresCount += 1;
                cardValues.level += 5;
            }
        }

        return base.EffectOnUse(targetNetId);
    }

    internal override void FightEndEffect()
    {
        (cardValues as MonsterValue).treasuresCount = 1;
        cardValues.level = 5;
    }
}
