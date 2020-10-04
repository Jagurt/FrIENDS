using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffCard : TreasureCard
{
    public Entity entityToApplyOn;

    internal override void UseCard()
    {
        PlayerInGame localPlayer = PlayerInGame.localPlayerInGame;

        if (!serverGameManager.fightInProggres)
        {
            GetComponent<Draggable>().ReturnToParent();
            Debug.Log("Can't use buffs outside battle!");
            return;
        }

        switch (GetComponent<BuffCard>().entityToApplyOn)
        {
            case Entity.Player: // Not Implemented Yet
                break;
            case Entity.Monster:
                if (localPlayer.table.GetComponentsInChildren<MonsterCard>().Length > 1) // If there are multiple monsters in battle
                {
                    localPlayer.choicePanel.PrepareToReceiveObjects(ChoicePanelTitle.ChooseMonsterToBuff);

                    foreach (var monster in localPlayer.table.GetComponent<TableDropZone>().BorrowMonsterCards())
                        localPlayer.choicePanel.ReceiveObjectToChoose(monster); // Create placeholders of monsters on table and send monsters to choice panel
                }
                else
                {
                    Debug.Log("ChooseBuffTarget: BuffCard - " + gameObject);
                    localPlayer.UseCard(this.netId, serverGameManager.fightingMonsters[0].GetComponent<MonsterCard>().netId); // Apply Buff to only Monster in battle
                }
                break;
            case Entity.Both: // Not Implemented Yet
                break;
            default:
                break;
        }
    }

    virtual internal void DispellEffect()
    {

    }
}
