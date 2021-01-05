#pragma warning disable CS0618 // Typ lub składowa jest przestarzała

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

enum Target { Player, Monster, Any };
/// <summary> Base effect class for all effect cards.  </summary>
public class Effect : Card
{
    [SerializeField] internal Target target;
    [SerializeField] internal bool choosable = false;

    private void Start()
    {
        gameManager = GameManager.singleton;
        Initialize();
    }

    internal override void UseCard()
    {
        PlayerInGame localPlayer = PlayerInGame.localPlayerInGame;
        // If target can be chosen
        if (choosable)
        {
            //Debug.Log("Putting choosable card on table");
            transform.SetParent(localPlayer.handContent);
            // Storing card to use it upon choosing target
            localPlayer.storedObject = this.gameObject;

            switch (target)
            {
                case Target.Player:
                    ChoicePanel.PrepareToReceiveObjects(ChoicePanelTitle.ChoosePlayerTarget);
                    ChoicePanel.ReceivePlayersToChoose();
                    break;
                case Target.Monster:
                    if (TableDropZone.singleton.transform.GetComponentsInChildren<MonsterCard>().Length > 1) // If there are multiple monsters in battle
                    {
                        ChoicePanel.PrepareToReceiveObjects(ChoicePanelTitle.ChooseMonsterToBuff);
                        foreach (var monster in TableDropZone.singleton.transform.GetComponent<TableDropZone>().BorrowMonsterCards())
                            ChoicePanel.ReceiveObjectToChoose(monster); // Create placeholders of monsters on table and send monsters to choice panel
                    }
                    else
                    {
                        localPlayer.UseStoredCardOnTarget(gameManager.fightingMonsters[0].GetComponent<Card>().GetNetId()); // Apply Buff to only Monster in battle
                    }
                    break;
                case Target.Any:
                    ChoicePanel.PrepareToReceiveObjects(ChoicePanelTitle.ChooseFightingTarget);
                    foreach (var monster in TableDropZone.singleton.transform.GetComponent<TableDropZone>().BorrowMonsterCards())
                        ChoicePanel.ReceiveObjectToChoose(monster);
                    ChoicePanel.ReceiveFightingPlayersToChoose();
                    break;
                default:
                    break;
            }
        }
        else
            localPlayer.UseCardOnLocalPlayer(this.netId);
    }

    /// <summary> Called when buff is removed from a monster. </summary>
    virtual internal void DispellEffect()
    {
        throw new System.NotImplementedException();
    }

    internal void RpcApplyOnPlayer()
    {
        if (targetNetId == gameManager.fightingPlayerNetId)
            gameManager.fightingPlayerEffects.Add(this);
        else if (targetNetId == gameManager.helpingPlayerNetId)
            gameManager.helpingPlayerEffects.Add(this);
    }

    internal void RpcApplyOnMonster()
    {
        GameObject monster = ClientScene.FindLocalObject(targetNetId);             // Finding Monsters and Buffs Objects
        monster.GetComponent<MonsterCard>().ClientApplyEffect(gameObject);   // Adding buff to Monsters Applied Buffs List
    }

    [Server]
    virtual internal IEnumerator ServerOnTurnEnd()
    {
        throw new System.NotImplementedException();
    }
}
