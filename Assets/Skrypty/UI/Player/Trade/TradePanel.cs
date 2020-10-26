using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TradePanel : MonoBehaviour
{
    internal static TradePanel tradePanel;

    static PlayerInGame playerWeTradeWith;
    internal static Transform opponentsCardsPanel;
    internal static Transform playersCardsPanel;
    static TextMeshProUGUI tradePanelTitle;

    static bool enemyAcceptedTrade;
    static bool enemyConfirmedTrade;

    TradePanel()
    {
        tradePanel = this;
    }

    internal static void Initialize()
    {
        opponentsCardsPanel = tradePanel.transform.Find("EnemysCardsPanel");
        playersCardsPanel = tradePanel.transform.Find("PlayersCardsPanel");
        tradePanelTitle = tradePanel.transform.Find("TradePanelTitle").GetComponent<TextMeshProUGUI>();
    }

    //private void Start()
    //{
    //    tradePanel = this;
    //    opponentsCardsPanel = tradePanel.transform.Find("EnemysCardsPanel");
    //    playersCardsPanel = tradePanel.transform.Find("PlayersCardsPanel");
    //    tradePanelTitle = tradePanel.transform.Find("TradePanelTitle").GetComponent<TextMeshProUGUI>();
    //}

    internal static void PrepareForTrade( PlayerInGame playerWeTradeWith )
    {
        tradePanel.gameObject.SetActive(true);
        TradePanel.playerWeTradeWith = playerWeTradeWith;
        tradePanel.GetComponentInChildren<TradeDropZone>().playerWeTradeWith = playerWeTradeWith;
        ResetAcceptance();
    }

    internal static void ReceiveEnemysCard( GameObject enemysCard )
    {
        enemysCard.transform.SetParent(opponentsCardsPanel);
        ResetAcceptance();
    }

    //internal void RemoveEnemysCard( GameObject enemysCard )
    //{
    //    Transform enemyTransform = GetComponentInChildren<TradeDropZone>().playerWeTradeWith.transform;
    //    enemysCard.transform.SetParent(enemyTransform);
    //    ResetAcceptance();
    //}

    internal void AcceptTrade( bool accepted ) // Changing players trade acceptance state
    {
        PlayerInGame.localPlayerInGame.AcceptTrade(playerWeTradeWith);

        if (accepted && enemyConfirmedTrade)// If player has already accepted trade and enemy has confirmed trade, trade needs to be finalized
        {
            PlayerInGame.localPlayerInGame.FinalizeTrade(playerWeTradeWith);
        }
    }

    internal void CancelTrade()
    {
        PlayerInGame.localPlayerInGame.CancelTrade(playerWeTradeWith);
    }

    internal static void TradeAcceptance() // Changing opponents trade acceptance state
    {
        if (enemyAcceptedTrade)
        {
            tradePanelTitle.text = "Confirmed";
            tradePanelTitle.color = Color.green;
            enemyConfirmedTrade = true;
        }
        else if (!enemyAcceptedTrade)
        {
            tradePanelTitle.text = "Accepted";
            tradePanelTitle.color = Color.yellow;
            enemyAcceptedTrade = true;
        }
    }

    internal static void ResetAcceptance()
    {
        tradePanelTitle.text = "Trading...";
        tradePanelTitle.color = Color.white;

        enemyAcceptedTrade = false;
        enemyConfirmedTrade = false;

        AcceptTradeButton.ResetButton();
    }

    internal static void Deactivate()
    {
        tradePanel.gameObject.SetActive(false);
    }
}
