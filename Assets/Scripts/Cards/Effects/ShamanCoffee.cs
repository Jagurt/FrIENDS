#pragma warning disable CS0618 // Typ lub składowa jest przestarzała
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ShamanCoffee : Effect
{
    private void Start()
    {
        gameManager = GameManager.singleton;
        target = Target.Any;
        choosable = true;
        Initialize();
    }

    internal override void UseCard()
    {
        if (!gameManager.fightInProggres)
            InfoPanel.AlertCannotUseCard();
        else
            base.UseCard();
    }

    [Server]
    internal override IEnumerator EffectOnUse()
    {
        yield return base.EffectOnUse();

        CustomNetManager CNM = CustomNetManager.singleton;

        if (CNM.isServerBusy)
            yield return new WaitUntil(() => !CNM.isServerBusy);
        CNM.isServerBusy = true;

        level = Random.Range(-5, 5);

        GameObject target = ClientScene.FindLocalObject(targetNetId);
        Debug.Log("target - " + target);
        MonoBehaviour targetScript = null;

        if (targetScript = target.GetComponent<PlayerInGame>())
        {
            gameManager.fightingPlayersLevel += level;

            yield return new WaitForEndOfFrame();
            RpcApplyOnPlayer();
        }
        else if (targetScript = target.GetComponent<MonsterCard>())
        {
            gameManager.fightingMonstersLevel += level;
            RpcApplyOnMonster();
        }
        else
            Debug.LogError("Wrong Entity Chosen!");

        yield return new WaitForEndOfFrame();
        CNM.isServerBusy = false;
    }
}
