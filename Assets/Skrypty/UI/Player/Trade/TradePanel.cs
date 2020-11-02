using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TradePanel : MonoBehaviour
{
    internal static TradePanel tradePanel;

    internal static PlayerInGame playerWeTradeWith;
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

    internal static void PrepareForTrade( PlayerInGame playerWeTradeWith )
    {
        tradePanel.gameObject.SetActive(true);
        TradePanel.playerWeTradeWith = playerWeTradeWith;
        ResetAcceptance();
    }

    internal static void ReceiveOpponentsCard( GameObject opponentsCard )
    {
        opponentsCard.transform.SetParent(opponentsCardsPanel);
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
    /// <summary>
    /// Advances state of acceptance to next stage.
    /// Accepting trade has 3 states: unaccepted, accepted and confirmed.
    /// Unaccepted is default state, set at the beginning and after adding or removing cards to trade.
    /// Accepted is first state of acceptance, it is achieved by pressing the Accept Button for first time.
    /// Confirmed is second state of acceptance, it is achieved by pressing the Accept Button for second time.
    /// </summary>
    internal static void TradeAcceptance()
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
    /// <summary> Resetting acceptance state to unaccepted. </summary>
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
