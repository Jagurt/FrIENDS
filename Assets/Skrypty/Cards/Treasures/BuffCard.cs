using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffCard : TreasureCard
{
    public Entity entityToApplyOn;

    /// <summary>
    /// If fight is in progress apply buff on monster.
    /// If there are more monsters choose monster to apply the buff on.
    /// </summary>
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
                if (TableDropZone.tableDropZone.transform.GetComponentsInChildren<MonsterCard>().Length > 1) // If there are multiple monsters in battle
                {
                    ChoicePanel.PrepareToReceiveObjects(ChoicePanelTitle.ChooseMonsterToBuff);

                    foreach (var monster in TableDropZone.tableDropZone.transform.GetComponent<TableDropZone>().BorrowMonsterCards())
                        ChoicePanel.ReceiveObjectToChoose(monster); // Create placeholders of monsters on table and send monsters to choice panel
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

    /// <summary>
    /// Called when buff is removed from a monster.
    /// </summary>
    virtual internal void DispellEffect()
    {
        
    }
}
