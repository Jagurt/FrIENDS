using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TradePanel : MonoBehaviour
{
    internal static TradePanel tradePanel;

    PlayerInGame playerWeTradeWith;
    internal Transform opponentsCardsPanel;
    internal Transform playersCardsPanel;
    TextMeshProUGUI tradePanelTitle;

    [SerializeField] bool enemyAcceptedTrade;
    [SerializeField] bool enemyConfirmedTrade;

    private void Awake()
    {
        //tradePanel = this;
        //opponentsCardsPanel = transform.Find("EnemysCardsPanel");
        //playersCardsPanel = transform.Find("PlayersCardsPanel");
        //tradePanelTitle = transform.Find("TradePanelTitle").GetComponent<TextMeshProUGUI>();
    }

    internal void Initialize()
    {
        tradePanel = this;
        opponentsCardsPanel = transform.Find("EnemysCardsPanel");
        playersCardsPanel = transform.Find("PlayersCardsPanel");
        tradePanelTitle = transform.Find("TradePanelTitle").GetComponent<TextMeshProUGUI>();
        gameObject.SetActive(false);
    }

    internal void PrepareForTrade( PlayerInGame playerWeTradeWith )
    {
        gameObject.SetActive(true);
        this.playerWeTradeWith = playerWeTradeWith;
        this.GetComponentInChildren<TradeDropZone>().playerWeTradeWith = playerWeTradeWith;
        ResetAcceptance();
    }

    internal void ReceiveEnemysCard( GameObject enemysCard )
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

    internal void AcceptTrade(bool accepted ) // Changing players trade acceptance state
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

    internal void TradeAcceptance() // Changing opponents trade acceptance state
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

    internal void ResetAcceptance()
    {
        tradePanelTitle.text = "Trading...";
        tradePanelTitle.color = Color.white;

        enemyAcceptedTrade = false;
        enemyConfirmedTrade = false;

        GetComponentInChildren<AcceptTradeButton>().ResetButton();
    }

    internal void Rest()
    {
        gameObject.SetActive(false);
    }
}
