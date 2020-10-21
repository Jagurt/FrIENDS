using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TradeDropZone : DropZone
{
    [SerializeField] internal PlayerInGame playerWeTradeWith;
    
    private void Start()
    {
        localPlayerInGame = PlayerInGame.localPlayerInGame;
    }

    public void OnBeginDrag(GameObject draggedCard) // Card is being picked up from playersCardsPanel
    {
        // Remove card from enemy's tradePanel
        localPlayerInGame.RemoveTradingCard(draggedCard);
    }

    public override void OnDrop( PointerEventData eventData )// Card is being dropped to playersCardsPanel
    {
        base.OnDrop(eventData);
        // Add card to enemy's tradePanel
        TradePanel.ResetAcceptance();
        localPlayerInGame.ReceiveTradingCard(eventData.pointerDrag, playerWeTradeWith);
    }

    //public override void OnPointerEnter( PointerEventData eventData )
    //{
    //    base.OnPointerEnter(eventData);
    //}

    //public override void OnPointerExit( PointerEventData eventData )
    //{
    //    base.OnPointerExit(eventData);
    //}
}
