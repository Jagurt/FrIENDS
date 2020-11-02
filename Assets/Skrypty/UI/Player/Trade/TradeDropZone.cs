﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TradeDropZone : DropZone
{
    private void Start()
    {
        localPlayerInGame = PlayerInGame.localPlayerInGame;
    }
    // Card is being picked up from playersCardsPanel
    public void OnBeginDrag(GameObject draggedCard) 
    {
        // Remove card from opponent's tradePanel
        localPlayerInGame.RemoveTradingCard(draggedCard);
    }
    // Card is being dropped to playersCardsPanel
    public override void OnDrop( PointerEventData eventData )
    {
        base.OnDrop(eventData);
        // Add card to enemy's tradePanel
        TradePanel.ResetAcceptance();
        localPlayerInGame.SendTradingCard(eventData.pointerDrag);
    }
}
