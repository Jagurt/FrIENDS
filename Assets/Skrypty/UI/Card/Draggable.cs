#pragma warning disable CS0618 // Type too old lul

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Draggable : NetworkBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] internal Transform parentToReturnTo = null;
    [SerializeField] internal Transform placeholderParent = null;
    [SerializeField] private GameObject placeholder = null;
    [SerializeField] private ServerDecksManager serverDecksManager;
    [SerializeField] private ServerGameManager serverGameManager;

    public GameObject Placeholder { get => placeholder; }

    private void Start()
    {
        serverGameManager = ServerGameManager.serverGameManager;
        StartCoroutine(GetAdopted());
    }

    /// <summary> Assign card which owns this script to correct deck. </summary>
    IEnumerator GetAdopted()
    {
        // Wait for serverGameManager
        while (!serverGameManager)
        {
            serverGameManager = ServerGameManager.serverGameManager;
            yield return new WaitForEndOfFrame();
        }
        // Wait for serverDecksManager
        while (!serverDecksManager)
        {
            serverDecksManager = ServerGameManager.serverGameManager.ServerDecksManager;
            yield return new WaitForEndOfFrame();
        }

        // Wait for Decks
        while (!serverDecksManager.DoorsDeck)
            yield return new WaitForEndOfFrame();
        while (!serverDecksManager.TreasuresDeck)
            yield return new WaitForEndOfFrame();
        while (!serverDecksManager.HelpHandsDeck)
            yield return new WaitForEndOfFrame();
        while (!serverDecksManager.SpellsDeck)
            yield return new WaitForEndOfFrame();

        // Put card to correct deck.
        switch (GetComponent<Card>().cardValues.deck)
        {
            case Deck.Doors:
                serverDecksManager.DoorsDeck.GetComponent<DrawCardZone>().ReceiveCard(this.transform);
                break;
            case Deck.Treasures:
                serverDecksManager.TreasuresDeck.GetComponent<DrawCardZone>().ReceiveCard(this.transform);
                break;
            case Deck.HelpingHand:
                serverDecksManager.HelpHandsDeck.GetComponent<DrawCardZone>().ReceiveCard(this.transform);
                break;
            case Deck.Spells:
                serverDecksManager.SpellsDeck.GetComponent<DrawCardZone>().ReceiveCard(this.transform);
                break;
        }
    }

    /// <summary> 
    /// Called when cards has started being dragged.
    /// Card can be dragged from players Hand, Trade Panel and Equipment Slot.
    /// </summary>
    public void OnBeginDrag( PointerEventData eventData )
    {
        //Debug.Log("OnBeginDrag(): Dragged Object - " + eventData.pointerDrag.gameObject);
        
        // If card is dragged from Trade Panel.
        TradeDropZone tradeDropZone;
        if (tradeDropZone = eventData.pointerDrag.gameObject.GetComponentInParent<TradeDropZone>())
            tradeDropZone.OnBeginDrag(eventData.pointerDrag.gameObject);

        // Create cards placeholder to be able to change its position in hand.
        CreatePlaceholder();

        // Disable catching cursor for this card during dragging to prevent unintended behaviour.
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    /// <summary>
    /// Create cards placeholder to be able to change its position in hand.
    /// </summary>
    void CreatePlaceholder()
    {
        // Create Placeholder as empty gamobject
        placeholder = new GameObject();

        // Set Placeholder parent to players hand.
        placeholder.transform.SetParent(PlayerInGame.localPlayerInGame.handContent);
        // Set parentToReturnTo to handPanel so if it is dropped wrongly it goes back to players hand.
        parentToReturnTo = PlayerInGame.localPlayerInGame.handContent;
        // Set placeholderParent to handPanel too so player can set its position in hand.
        placeholderParent = parentToReturnTo;
        // Set Placeholder position in hand to this cards position in hand.
        placeholder.transform.SetSiblingIndex(this.transform.GetSiblingIndex());

        // Set placeholders size to be same as this cards.
        LayoutElement layoutElement = placeholder.AddComponent<LayoutElement>();            
        layoutElement.preferredWidth = this.GetComponent<LayoutElement>().preferredWidth;
        layoutElement.preferredHeight = this.GetComponent<LayoutElement>().preferredHeight;

        // Set this cards placeholder to be PlayerCanvas so that this dragged card is in front of all other UI objects.
        this.transform.SetParent(PlayerInGame.playerCanvas.transform);
    }
    /// <summary>
    /// Called while dragging this card.
    /// </summary>
    public void OnDrag( PointerEventData eventData )
    {
        // Debug.Log("Dragged object - " + eventData.pointerDrag.gameObject);

        // Change this cards position to cursor position.
        this.transform.position = eventData.position;

        // When card is being dragged over no UI objects or when object that card is being dragged over isn't DropZone.
        if (eventData.pointerCurrentRaycast.gameObject != null && !eventData.pointerCurrentRaycast.gameObject.GetComponent<DropZone>())
            return;

        // Updating placeholders parent.
        if (placeholder.transform.parent != placeholderParent)
            placeholder.transform.SetParent(placeholderParent);

        // Finding cards position in hand based on its position compared to other cards position.
        int newSiblingIndex = placeholderParent.childCount;
        for (int i = 0; i < placeholderParent.childCount; i++)
        {
            if (this.transform.position.x < placeholderParent.GetChild(i).position.x)
            {
                newSiblingIndex = i;

                if (placeholder.transform.GetSiblingIndex() < newSiblingIndex)
                    newSiblingIndex--;

                break;
            }
        }
        // Setting new position in hand.
        placeholder.transform.SetSiblingIndex(newSiblingIndex);
    }

    public void OnEndDrag( PointerEventData eventData )
    {
        // If card is dropped on no UI objects, on its former parent or UI object that is not meant for dropping cards.
        if (
            eventData.pointerCurrentRaycast.gameObject == null ||
            eventData.pointerCurrentRaycast.gameObject.transform == parentToReturnTo ||
            eventData.pointerCurrentRaycast.gameObject.GetComponent<DropZone>() == null
            )
            ReturnToParent();

        // Returning ablity to be clicked.
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        if (placeholder)
            Destroy(placeholder);
    }

    /// <summary> Returning card to its former parent. </summary>
    public void ReturnToParent()
    {
        this.transform.SetParent(parentToReturnTo);

        if (placeholder)
            this.transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());

        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
}
