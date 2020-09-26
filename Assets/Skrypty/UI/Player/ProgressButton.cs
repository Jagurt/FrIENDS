using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressButton : MonoBehaviour
{
    Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(delegate { OnClick(); });
    }

    public void OnClick()
    {
        if(!ServerGameManager.serverGameManager.fightInProggres && PlayerInGame.localPlayerInGame.serverGameManager.turnPhase == TurnPhase.Search)
        {
            PlayerInGame.localPlayerInGame.EndTurn();
            return;
        }

        PlayerInGame.localPlayerInGame.CmdReadyPlayersUp(); // Report Readiness, and disable button
        button.interactable = false;
    }

    internal void ActivateButton()
    {
        button.interactable = true;
    }
    internal void DeactivateButton()
    {
        button.interactable = false;
    }

}
