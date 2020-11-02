using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum Target { Player, Monster, All };
/// <summary> Base effect class for all effect cards.  </summary>
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
        // If target can be chosen
        if (choosable) 
        {
            //Debug.Log("Putting choosable card on table");
            localPlayer.PutCardOnTable(this.netId);
            // Storing card to use it upon choosing target
            localPlayer.storedObject = gameObject; 

            switch (target)
            {
                case Target.Player:
                    ChoicePanel.PrepareToReceiveObjects(ChoicePanelTitle.ChoosePlayer);
                    ChoicePanel.ReceivePlayersToChoose(FindObjectsOfType<PlayerInGame>());
                    break;
                case Target.Monster:
                    // Not Implemented yet
                    break;
                case Target.All:
                    // Not Implemented yet
                    break;
                default:
                    break;
            }
        }
        else
            localPlayer.UseCardOnLocalPlayer(this.netId);
    }
}
