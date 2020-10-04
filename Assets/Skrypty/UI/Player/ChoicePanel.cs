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
    [SerializeField] GameObject cardChoicePlaceholder;
    [SerializeField] bool worksOnPlayer;
    [SerializeField] Transform objectsContainer;
    [SerializeField] TextMeshProUGUI titleTMP;
    ChoicePanelTitle panelTitle;

    List<GameObject> placeholders = new List<GameObject>();

    string[] titles = {
        "Choose first doors in your turn",  // ChoicePanelTitle.FirstDoor
        "Choose Card",                      // ChoicePanelTitle.ChooseCard
        "Choose Monster",                   // ChoicePanelTitle.ChooseMonster
        "Choose Player",                    // and so on...
        "Choose Player to trade"
    };

    private void Start()
    {
        //objectsContainer = transform.Find("ObjectsContainer");
        //titleTMP = transform.Find("TitleText").GetComponent<TextMeshProUGUI>();
    }

    public void Initialize()
    {
        objectsContainer = transform.Find("ObjectsContainer");
        titleTMP = transform.Find("TitleText").GetComponent<TextMeshProUGUI>();
        this.gameObject.SetActive(false);
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
            placeholderScript.heldObject.GetComponent<CanvasGroup>().blocksRaycasts = true; // Enabling dragging of cards after chosing

            if (placeholderScript.heldObject != chosenDoors) // Setting unselected cards to discard
                cardsToDiscard.Add(placeholderScript.heldObject);

            Destroy(placeholderScript.gameObject); // Destroying placeholders
        }

        PlayerInGame.localPlayerInGame.RequestDiscardCards(cardsToDiscard); // Executing discarding cards
        PlayerInGame.localPlayerInGame.storedObject = chosenDoors;

        if (worksOnPlayer)
        {
            // Debug.Log("ChoicePanel:ChooseDoors - worksOnPlayer - " + worksOnPlayer);
            PlayerInGame.localPlayerInGame.UseCardOnTarget(PlayerInGame.localPlayerInGame.gameObject); // if chosen card works on local player, use it on them.
            PlayerInGame.localPlayerInGame.ProgressButton.ActivateButton();
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

    internal void ReceiveObjectToChoose( GameObject receivedObject )
    {
        GameObject placeholder = Instantiate(cardChoicePlaceholder);

        placeholders.Add(placeholder);
        
        placeholder.GetComponent<CardChoicePlaceholder>().Initialize(this, receivedObject, objectsContainer);
    }

    internal void ReceivePlayersToChoose( PlayerInGame[] players )
    {
        foreach (var player in players)
        {
            Debug.Log("player.opponentInPanel - " + player.opponentInPanel);
            GameObject enemyInPanel = Instantiate(player.opponentInPanel);
            enemyInPanel.GetComponent<OpponentInPanel>().InitializeInChoicePanel(player);
            ReceiveObjectToChoose(enemyInPanel);
        }
    }

    internal void SetWhichToChoose()
    {
        throw new NotImplementedException();
    }

    internal void PrepareToReceiveObjects( ChoicePanelTitle panelTitle, bool worksOnPlayer = false )
    {
        this.gameObject.SetActive(true);
        this.worksOnPlayer = worksOnPlayer;

        this.panelTitle = panelTitle;
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
