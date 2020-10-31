using System;
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
    ChoicePanelTitle panelTitle;

    [SerializeField] GameObject cardChoicePlaceholder;
    [SerializeField] bool worksOnPlayer;

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

    internal void Choose( GameObject chosenObject )
    {
        switch (panelTitle)
        {
            case ChoicePanelTitle.DrawFirstDoor:
                ChooseDoors(chosenObject);
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

    void ChooseDoors( GameObject chosenDoors )
    {
        List<GameObject> cardsToDiscard = new List<GameObject>();

        foreach (var placeholderScript in this.GetComponentsInChildren<CardChoicePlaceholder>())
        {
            placeholderScript.heldObject.transform.SetParent(PlayerInGame.localPlayerInGame.transform); // Throwing cards of choice panel to avoid unintended behaviour
            placeholderScript.heldObject.GetComponent<Draggable>().enabled = true; // Enabling dragging of cards after chosing

            if (placeholderScript.heldObject != chosenDoors) // Setting unselected cards to discard
                cardsToDiscard.Add(placeholderScript.heldObject);

            Destroy(placeholderScript.gameObject); // Destroying placeholders
        }

        StartCoroutine(PlayerInGame.localPlayerInGame.DiscardCard(cardsToDiscard)); // Executing discarding cards
        PlayerInGame.localPlayerInGame.storedObject = chosenDoors;

        if (worksOnPlayer)
        {
            // Debug.Log("ChoicePanel:ChooseDoors - worksOnPlayer - " + worksOnPlayer);
            PlayerInGame.localPlayerInGame.UseCardOnTarget(PlayerInGame.localPlayerInGame.gameObject); // if chosen card works on local player, use it on them.
            PlayerInGame.localPlayerInGame.progressButton.ActivateButton();
        }
        else
            chosenDoors.GetComponent<Card>().UseCard(); // if not choose target for chosen card.
    }

    void ChooseMonster( GameObject chosenMonster )
    {
        TableDropZone.tableDropZone.ReturnBorrowedCards();
        //ServerGameManager.serverGameManager.targetForCardToUseNetId = chosenMonster.GetComponent<NetworkIdentity>().netId;
        // TODO: Dokończyć wybieranie potworów

        // PlayerInGame.localPlayerInGame.ApplyBuff(chosenMonster.GetComponent<Card>().netId);
    }

    void ChoosePlayerToTrade( OpponentInPanel enemyInPanel )
    {
        Debug.Log("ChoosePlayerToTrade: enemyInPanel - " + enemyInPanel);
        PlayerInGame.localPlayerInGame.RequestTrade(enemyInPanel.storedPlayer);
    }

    void ChoosePlayer( OpponentInPanel enemyInPanel )
    {
        PlayerInGame.localPlayerInGame.UseCardOnTarget(enemyInPanel.storedPlayer.gameObject);
    }

    internal static void ReceiveObjectToChoose( GameObject receivedObject )
    {
        GameObject placeholder = Instantiate(choicePanel.cardChoicePlaceholder);

        placeholders.Add(placeholder);
        
        placeholder.GetComponent<CardChoicePlaceholder>().Initialize(choicePanel, receivedObject, objectsContainer);
    }

    internal static void ReceivePlayersToChoose( PlayerInGame[] players )
    {
        foreach (var player in players)
        {
            Debug.Log("player.opponentInPanel - " + player.opponentInPanel);
            GameObject enemyInPanel = Instantiate(player.opponentInPanelPrefab);
            enemyInPanel.GetComponent<OpponentInPanel>().InitializeInChoicePanel(player);
            ReceiveObjectToChoose(enemyInPanel);
        }
    }

    internal static void SetWhichToChoose()
    {
        throw new NotImplementedException();
    }

    internal static void PrepareToReceiveObjects( ChoicePanelTitle panelTitle, bool worksOnPlayer = false )
    {
        choicePanel.gameObject.SetActive(true);
        choicePanel.worksOnPlayer = worksOnPlayer;
        choicePanel.panelTitle = panelTitle;

        if (!titleTMP)
        {
            objectsContainer = choicePanel.transform.Find("ObjectsContainer");
            titleTMP = choicePanel.transform.Find("TitleText").GetComponent<TextMeshProUGUI>();
        }
        titleTMP.text = titles[(int)panelTitle];
    }

    void DestroyPlaceholders()
    {
        foreach (var placeholder in placeholders)
        {
            Debug.Log("Destroying placeholder - " + placeholder);
            Destroy(placeholder.gameObject);
        }

        //StartCoroutine(ClearPlaceholders());
    }

    void Rest()
    {
        DestroyPlaceholders();
        this.worksOnPlayer = false;
        this.gameObject.SetActive(false);
    }
}
