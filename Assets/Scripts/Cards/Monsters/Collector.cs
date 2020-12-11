#pragma warning disable CS0618 // Type too old lul

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Collector : MonsterCard
{
    void Start()
    {
        gameManager = GameManager.singleton;
        Initialize();
    }

    /// <summary> Increase power of a monster for each "Glass" object that player is wearing. </summary>
    [Server]
    override internal IEnumerator EffectOnUse()
    {
        ServerGetExcited();

        return base.EffectOnUse();
    }

    [Server]
    void ServerGetExcited()
    {
        PlayerInGame fightingPlayer = ClientScene.FindLocalObject(targetNetId).GetComponent<PlayerInGame>();
        cardValues.level = 5;

        foreach (var item in fightingPlayer.equippedItems)
            if (item.GetComponent<EquipmentCard>().cardValues.name.Contains("Glass"))
            {
                (cardValues as MonsterValue).treasuresCount += 1;
                cardValues.level += 5;
            }

        if (gameManager.helpingPlayerNetId != NetworkInstanceId.Invalid)
        {
            fightingPlayer = ClientScene.FindLocalObject(gameManager.helpingPlayerNetId).GetComponent<PlayerInGame>();

            foreach (var item in fightingPlayer.equippedItems)
                if (item.GetComponent<EquipmentCard>().cardValues.name.Contains("Glass"))
                {
                    (cardValues as MonsterValue).treasuresCount += 1;
                    cardValues.level += 5;
                }
        }
    }

    [Server]
    override internal void ServerEquipmentCheck()
    {
        Debug.Log("Eq check");
        ServerGetExcited();
        base.ServerEquipmentCheck();
    }

    protected override void FightEndEffect()
    {
        (cardValues as MonsterValue).treasuresCount = 1;
        cardValues.level = 5;
    }
}
