#pragma warning disable CS0618 // Typ lub składowa jest przestarzała

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

enum ChoicePanelTitle { DrawFirstDoor, ChooseCard, ChooseMonsterToBuff, ChoosePlayerToHelp, ChoosePlayerTarget, ChooseFightingTarget, ChoosePlayerToTrade }

public class ChoicePanel : MonoBehaviour
{
    internal static ChoicePanel singleton;
    ChoicePanel()
    {
        singleton = this;
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
        "Choose Monster",                   // ChoicePanelTitle.ChooseMonsterToBuff
        "Choose Player to help",            // ChoicePanelTitle.ChoosePlayerToHelp
        "Choose Player Target",             // ChoicePanelTitle.ChoosePlayerTarget
        "Choose Fighting Target",           // ChoicePanelTitle.ChooseFightingTarget
        "Choose Player to trade"            // ChoicePanelTitle.ChoosePlayerToTrade
    };

    private void Start()
    {
        objectsContainer = singleton.transform.Find("ObjectsContainer");
        titleTMP = singleton.transform.Find("TitleText").GetComponent<TextMeshProUGUI>();
    }

    internal static void PrepareToReceiveObjects( ChoicePanelTitle panelTitle, bool worksOnPlayer = false )
    {
        singleton.gameObject.SetActive(true);
        ChoicePanel.worksOnPlayer = worksOnPlayer;
        ChoicePanel.panelTitle = panelTitle;

        if (!titleTMP)
        {
            objectsContainer = singleton.transform.Find("ObjectsContainer");
            titleTMP = singleton.transform.Find("TitleText").GetComponent<TextMeshProUGUI>();
        }
        titleTMP.text = titles[(int)panelTitle];
    }
    /// <summary> Creating placeholder for object to choose and initializing it in ChoicePanel </summary>
    internal static void ReceiveObjectToChoose( GameObject receivedObject )
    {
        GameObject placeholder = Instantiate(singleton.cardChoicePlaceholder);

        placeholders.Add(placeholder);
        placeholder.GetComponent<ChoicePlaceholder>().Initialize(singleton, receivedObject, objectsContainer);
    }
    /// <summary>
    /// Creating player stats UI ubjects for all players in ChoicePanel.
    /// And copying stats for corresponding UI object.
    /// </summary>
    internal static void ReceivePlayersToChoose( PlayerInGame[] players = null )
    {
        if (players == null)
            players = FindObjectsOfType<PlayerInGame>();

        foreach (var player in players)
        {
            Debug.Log("player.opponentInPanel - " + player.opponentStatsUI);
            ReceivePlayer(player);
        }
    }

    internal static void ReceiveFightingPlayersToChoose()
    {
        PlayerInGame fightingplayer = ClientScene.FindLocalObject(GameManager.singleton.fightingPlayerNetId).GetComponent<PlayerInGame>();
        ReceivePlayer(fightingplayer);

        if (GameManager.singleton.helpingPlayerNetId != NetworkInstanceId.Invalid)
        {
            fightingplayer = ClientScene.FindLocalObject(GameManager.singleton.helpingPlayerNetId).GetComponent<PlayerInGame>();
            ReceivePlayer(fightingplayer);
        }
    }

    static void ReceivePlayer(PlayerInGame playerScript)
    {
        GameObject playerStats = Instantiate(playerScript.opponentStatsUIPrefab);
        GameManager.singleton.StartCoroutine(playerStats.GetComponent<PlayerStats>().Initialize(playerScript));
        ReceiveObjectToChoose(playerStats);
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
            case ChoicePanelTitle.ChoosePlayerToHelp:
                ChoosePlayerToHelp(chosenObject.GetComponent<PlayerStats>());
                break;
            case ChoicePanelTitle.ChoosePlayerTarget:
                ChoosePlayerTarget(chosenObject.GetComponent<PlayerStats>());
                break;
            case ChoicePanelTitle.ChooseFightingTarget:
                ChooseAnyFighting(chosenObject);
                break;
            case ChoicePanelTitle.ChoosePlayerToTrade:
                ChoosePlayerToTrade(chosenObject.GetComponent<PlayerStats>());
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
        foreach (var placeholder in singleton.GetComponentsInChildren<ChoicePlaceholder>())
        {
            // Throwing cards out of choice panel to avoid unintended behaviour
            placeholder.heldObject.transform.SetParent(PlayerInGame.localPlayerInGame.transform);
            // Enabling dragging of cards after chosing
            placeholder.heldObject.GetComponent<Draggable>().enabled = true;
            // Setting unselected cards to discard
            if (placeholder.heldObject != chosenDoors)
                cardsToDiscard.Add(placeholder.heldObject);
            // Destroying placeholders
            Destroy(placeholder.gameObject);
        }

        foreach (var item in cardsToDiscard)
            Debug.Log("Set card to discard - " + item);

        // Executing discarding cards
        PlayerInGame.localPlayerInGame.DiscardCard(cardsToDiscard);
        // Debug.Log("ChoicePanel:ChooseFirstDoors - worksOnPlayer - " + worksOnPlayer);
        PlayerInGame.localPlayerInGame.ChooseFirstDoors(chosenDoors);
    }

    static void ChooseMonster( GameObject chosenMonster )
    {
        TableDropZone.singleton.RetrieveBorrowedCards();
        MonsterCard monsterCard = chosenMonster.GetComponent<MonsterCard>();

        PlayerInGame.localPlayerInGame.UseStoredCardOnTarget(monsterCard.GetNetId());
    }

    static void ChoosePlayerToTrade( PlayerStats enemyInPanel )
    {
        Debug.Log("ChoosePlayerToTrade: enemyInPanel - " + enemyInPanel);
        PlayerInGame.localPlayerInGame.RequestTrade(enemyInPanel.storedPlayer);
    }

    static void ChoosePlayerToHelp( PlayerStats enemyInPanel )
    {
        PlayerInGame.localPlayerInGame.ChoosePlayerToHelp(enemyInPanel.storedPlayer.gameObject);
    }

    static void ChoosePlayerTarget( PlayerStats enemyInPanel )
    {
        PlayerInGame.localPlayerInGame.UseStoredCardOnTarget(enemyInPanel.storedPlayer.netId);
    }

    static void ChooseAnyFighting( GameObject chosenTarget)
    {
        TableDropZone.singleton.RetrieveBorrowedCards();

        NetworkBehaviour chosenScript;
        if (chosenScript = chosenTarget.GetComponent<MonsterCard>())
            PlayerInGame.localPlayerInGame.UseStoredCardOnTarget((chosenScript as Card).GetNetId());
        else if (chosenScript = chosenTarget.GetComponent<PlayerStats>().storedPlayer)
            PlayerInGame.localPlayerInGame.UseStoredCardOnTarget(chosenScript.netId);
        else
            Debug.LogError("Wrong target chosen!");
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
        singleton.gameObject.SetActive(false);
    }
}
