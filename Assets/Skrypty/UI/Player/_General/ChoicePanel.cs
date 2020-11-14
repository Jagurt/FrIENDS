﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

enum ChoicePanelTitle { DrawFirstDoor, ChooseCard, ChooseMonsterToBuff, ChoosePlayer, ChoosePlayerToTrade }

public class ChoicePanel : MonoBehaviour
{
    internal static ChoicePanel choicePanel;
    ChoicePanel()
    {
        choicePanel = this;
    }

    static Transform objectsContainer;
    static TextMeshProUGUI titleTMP;
    static ChoicePanelTitle panelTitle;

    [SerializeField] GameObject cardChoicePlaceholder;
    [SerializeField] static bool worksOnPlayer;

    static List<GameObject> placeholders = new List<GameObject>();

    static string[] titles = {
        "Choose first doors in your turn",  // ChoicePanelTitle.FirstDoor
        "Choose Card",                      // ChoicePanelTitle.ChooseCard
        "Choose Monster",                   // ChoicePanelTitle.ChooseMonster
        "Choose Player",                    // and so on...
        "Choose Player to trade"
    };

    private void Start()
    {
        objectsContainer = choicePanel.transform.Find("ObjectsContainer");
        titleTMP = choicePanel.transform.Find("TitleText").GetComponent<TextMeshProUGUI>();
    }

    internal static void PrepareToReceiveObjects( ChoicePanelTitle panelTitle, bool worksOnPlayer = false )
    {
        choicePanel.gameObject.SetActive(true);
        ChoicePanel.worksOnPlayer = worksOnPlayer;
        ChoicePanel.panelTitle = panelTitle;

        if (!titleTMP)
        {
            objectsContainer = choicePanel.transform.Find("ObjectsContainer");
            titleTMP = choicePanel.transform.Find("TitleText").GetComponent<TextMeshProUGUI>();
        }
        titleTMP.text = titles[(int)panelTitle];
    }
    /// <summary> Creating placeholder for object to choose and initializing it in ChoicePanel </summary>
    internal static void ReceiveObjectToChoose( GameObject receivedObject )
    {
        GameObject placeholder = Instantiate(choicePanel.cardChoicePlaceholder);

        placeholders.Add(placeholder);
        placeholder.GetComponent<CardChoicePlaceholder>().Initialize(choicePanel, receivedObject, objectsContainer);
    }
    /// <summary>
    /// Creating player stats UI ubjects for all players in ChoicePanel.
    /// And copying stats for corresponding UI object.
    /// </summary>
    internal static void ReceivePlayersToChoose( PlayerInGame[] players )
    {
        foreach (var player in players)
        {
            Debug.Log("player.opponentInPanel - " + player.opponentStatsUI);
            GameObject enemyInPanel = Instantiate(player.opponentStatsUIPrefab);
            enemyInPanel.GetComponent<OpponentInPanel>().InitializeInChoicePanel(player);
            ReceiveObjectToChoose(enemyInPanel);
        }
    }
    /// <summary> Performing certain action for chosen object based on panel title. </summary>
    /// <param name="chosenObject"></param>
    internal static void Choose( GameObject chosenObject )
    {
        switch (panelTitle)
        {
            case ChoicePanelTitle.DrawFirstDoor:
                ChooseFirstDoors(chosenObject);
                break;
            case ChoicePanelTitle.ChooseCard:
                break;
            case ChoicePanelTitle.ChooseMonsterToBuff:
                ChooseMonster(chosenObject);
                break;
            case ChoicePanelTitle.ChoosePlayer:
                ChoosePlayer(chosenObject.GetComponent<OpponentInPanel>());
                break;
            case ChoicePanelTitle.ChoosePlayerToTrade:
                ChoosePlayerToTrade(chosenObject.GetComponent<OpponentInPanel>());
                break;
            default:
                break;
        }

        Rest();
    }
    /// <summary> When choosing first doors in turn. </summary>
    static void ChooseFirstDoors( GameObject chosenDoors )
    {
        List<GameObject> cardsToDiscard = new List<GameObject>();
        // Finding cards to discard, all except chosen.
        foreach (var placeholderScript in choicePanel.GetComponentsInChildren<CardChoicePlaceholder>())
        {
            // Throwing cards out of choice panel to avoid unintended behaviour
            placeholderScript.heldObject.transform.SetParent(PlayerInGame.localPlayerInGame.transform);
            // Enabling dragging of cards after chosing
            placeholderScript.heldObject.GetComponent<Draggable>().enabled = true;
            // Setting unselected cards to discard
            if (placeholderScript.heldObject != chosenDoors)
                cardsToDiscard.Add(placeholderScript.heldObject);
            // Destroying placeholders
            Destroy(placeholderScript.gameObject);
        }
        // Executing discarding cards
        choicePanel.StartCoroutine(PlayerInGame.localPlayerInGame.DiscardCard(cardsToDiscard));

        // Debug.Log("ChoicePanel:ChooseFirstDoors - worksOnPlayer - " + worksOnPlayer);
        PlayerInGame.localPlayerInGame.ChooseFirstDoors(chosenDoors);
    }

    static void ChooseMonster( GameObject chosenMonster ) 
    { 
        TableDropZone.tableDropZone.ReturnBorrowedCards();
        MonsterCard monsterCard = chosenMonster.GetComponent<MonsterCard>();

        PlayerInGame.localPlayerInGame.UseStoredCardOnTarget(monsterCard.GetNetId());
    }
    static void ChoosePlayerToTrade( OpponentInPanel enemyInPanel )
    {
        Debug.Log("ChoosePlayerToTrade: enemyInPanel - " + enemyInPanel);
        PlayerInGame.localPlayerInGame.RequestTrade(enemyInPanel.storedPlayer);
    }
    static void ChoosePlayer( OpponentInPanel enemyInPanel )
    {
        PlayerInGame.localPlayerInGame.ChoosePlayerToHelp(enemyInPanel.storedPlayer.gameObject);
    }
    static void DestroyPlaceholders()
    {
        foreach (var placeholder in placeholders)
            Destroy(placeholder.gameObject);
    }
    /// <summary> Destroying placeholders and setting variables when completing choosing. </summary>
    static void Rest()
    {
        DestroyPlaceholders();
        worksOnPlayer = false;
        choicePanel.gameObject.SetActive(false);
    }
}
