using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressButton : MonoBehaviour
{
    internal static ProgressButton progressButton;

    static Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(delegate { OnClick(); });
    }

    internal static void Initialize()
    {
        progressButton = PlayerCanvas.playerCanvas.transform.Find("ProgressButton").GetComponent<ProgressButton>();
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

    internal static void ActivateButton()
    {
        button.interactable = true;
    }
    internal static void DeactivateButton()
    {
        button.interactable = false;
    }

}
