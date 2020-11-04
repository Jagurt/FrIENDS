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
        button.onClick.AddListener(() => OnClick());
    }

    /// <summary>
    /// Report Readiness, and disable button or end turn.
    /// </summary>
    public void OnClick()
    {
        if(!ServerGameManager.serverGameManager.fightInProggres && ServerGameManager.serverGameManager.turnPhase == TurnPhase.Search)
        {
            PlayerInGame.localPlayerInGame.EndTurn();
            return;
        }

        PlayerInGame.localPlayerInGame.CmdReadyPlayersUp(); 
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
