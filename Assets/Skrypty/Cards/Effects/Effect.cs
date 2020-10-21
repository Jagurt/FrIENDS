using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum Target { Player, Monster, All };

public class Effect : Card
{
    [SerializeField]internal Target target;
    internal bool choosable = false;

    private void Start()
    {
        serverGameManager = ServerGameManager.serverGameManager;
        Initialize();
    }

    internal override void UseCard()
    {
        PlayerInGame localPlayer = PlayerInGame.localPlayerInGame;

        if (choosable) // If card is an Effect and target is choosable
        {
            //Debug.Log("Putting choosable card on table");
            localPlayer.PutCardOnTable(this.netId);
            localPlayer.storedObject = gameObject; // Used Card is selected object

            switch (target)
            {
                case Target.Player:
                    ChoicePanel.PrepareToReceiveObjects(ChoicePanelTitle.ChoosePlayer);
                    ChoicePanel.ReceivePlayersToChoose(FindObjectsOfType<PlayerInGame>());
                    break;
                case Target.Monster:
                    break;
                case Target.All:
                    ChoicePanel.SetWhichToChoose(); // Not Implemented yet
                    break;
                default:
                    break;
            }
        }
        else
            localPlayer.UseCardOnLocalPlayer(this.netId);
    }
}
