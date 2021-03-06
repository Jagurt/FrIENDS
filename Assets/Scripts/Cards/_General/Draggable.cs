﻿#pragma warning disable CS0618 // Type too old lul

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
    private static GameObject placeholder = null;
    [SerializeField] private DecksManager serverDecksManager;
    [SerializeField] private GameManager serverGameManager;
    int _LTMovementId;

    private void Start()
    {
        serverGameManager = GameManager.singleton;
        StartCoroutine(GetAdopted());

        CreatePlaceholder();
    }

    void CreatePlaceholder()
    {
        if (placeholder)
            return;

        placeholder = new GameObject();
        placeholder.transform.SetParent(PlayerCanvas.singleton.transform);

        // Set placeholders size to be same as cards.
        LayoutElement layoutElement = placeholder.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = this.GetComponent<LayoutElement>().preferredWidth;
        layoutElement.preferredHeight = this.GetComponent<LayoutElement>().preferredHeight;
    }

    /// <summary> Assign card which owns this script to correct deck. </summary>
    IEnumerator GetAdopted()
    {
        // Wait for serverGameManager
        while (!serverGameManager)
        {
            serverGameManager = GameManager.singleton;
            yield return new WaitForEndOfFrame();
        }
        // Wait for serverDecksManager
        while (!serverDecksManager)
        {
            serverDecksManager = GameManager.singleton.ServerDecksManager;
            yield return new WaitForEndOfFrame();
        }

        // Wait for Decks
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
        if (transform.parent.tag == "BlockDragging")
            return;

        // If card is dragged from Trade Panel.
        TradeDropZone tradeDropZone;
        if (tradeDropZone = eventData.pointerDrag.gameObject.GetComponentInParent<TradeDropZone>())
            tradeDropZone.OnBeginDrag(eventData.pointerDrag.gameObject);

        // Create cards placeholder to be able to change its position in hand.
        MovePlaceholderToHand();

        // Set this cards placeholder to be PlayerCanvas so that this dragged card is in front of all other UI objects.
        this.transform.SetParent(PlayerInGame.playerCanvas.transform);

        // Disable catching cursor for this card during dragging to prevent unintended behaviour.
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    /// <summary> Create cards placeholder to be able to change its position in hand. </summary>
    void MovePlaceholderToHand()
    {
        parentToReturnTo = PlayerInGame.localPlayerInGame.handContent;
        // Set placeholderParent to handPanel too so player can set its position in hand.
        placeholderParent = parentToReturnTo;
        // Set Placeholder parent to players hand.
        placeholder.transform.SetParent(parentToReturnTo);
        // Set Placeholder position in hand to this cards position in hand.
        placeholder.transform.SetSiblingIndex(this.transform.GetSiblingIndex());
    }

    IEnumerator MovePlaceholderToParent( Transform placeholderParent )
    {
        // Set placeholderParent
        this.placeholderParent = placeholderParent;

        // Set Placeholders transform parent.
        placeholder.transform.SetParent(placeholderParent);
        placeholder.transform.SetAsLastSibling();

        yield return new WaitForEndOfFrame();
    }

    /// <summary> Called while dragging this card. </summary>
    public void OnDrag( PointerEventData eventData )
    {
        // Debug.Log("Dragged object - " + eventData.pointerDrag.gameObject);

        // Change this cards position to cursor position.
        this.transform.position = Vector3.Scale(Camera.main.ScreenToWorldPoint(eventData.position), new Vector3(1, 1, 0));

        // Updating placeholders parent.
        if (placeholder.transform.parent != placeholderParent)
            placeholder.transform.SetParent(placeholderParent);

        // When card is being dragged over no UI objects or when object that card is being dragged over isn't DropZone.
        if (eventData.pointerCurrentRaycast.gameObject == null || !eventData.pointerCurrentRaycast.gameObject.GetComponent<DropZone>())
        {
            placeholderParent = PlayerInGame.localPlayerInGame.handContent;
            return;
        }

        if (eventData.pointerCurrentRaycast.gameObject.GetComponent<TableDropZone>())
            placeholderParent = TableDropZone.cardQueueZone;

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
        if (eventData.pointerCurrentRaycast.gameObject == null || !eventData.pointerCurrentRaycast.gameObject.GetComponent<DiscardDropZone>())
            ReturnToParent();
    }

    /// <summary> Returning card to its former parent. </summary>
    public void ReturnToParent()
    {
        // Sliding card to new place parent

        Vector3 offset = new Vector3(50, -85, 0);

        if (placeholder)
        {
            Vector3 placeholderPosition = placeholder.GetComponent<Transform>().position;
            int placeholderSibIndex = placeholder.transform.GetSiblingIndex();
            FreePlaceholder();

            _LTMovementId = LeanTween.move(this.gameObject, placeholderPosition - offset, 0.1f)
                .setOnUpdate(( Vector3 val ) => transform.position = val)
                .setOnComplete(() =>
               {
                   this.transform.SetParent(placeholderParent);
                   this.transform.SetSiblingIndex(placeholderSibIndex);
               })
               .id;
        }

        // Returning ablity to be clicked.
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    internal void ClientSlide( Transform newParent )
    {
        ClientStopLeanMovement();
        transform.SetParent(PlayerCanvas.singleton.transform, true);

        Vector3 offset = new Vector3(50, -85, 0);

        _LTMovementId = LeanTween.move(this.gameObject, newParent.position - offset, 0.1f)
                .setOnUpdate(( Vector3 val ) => transform.position = val)
                .setOnComplete(() => this.transform.SetParent(newParent))
                .id;
    }

    internal IEnumerator ClientSlideWithNewPlaceholder( Transform placeholderParent )
    {
        ClientStopLeanMovement();

        yield return MovePlaceholderToParent(placeholderParent);

        transform.SetParent(PlayerCanvas.singleton.transform, true);

        Vector3 offset = new Vector3(50, -85, 0);
        Vector3 placeholderPosition = placeholder.GetComponent<Transform>().position;
        int placeholderSibIndex = placeholder.transform.GetSiblingIndex();

        yield return new WaitForEndOfFrame();

        FreePlaceholder();

        _LTMovementId = LeanTween.move(this.gameObject, placeholderPosition - offset, 0.1f)
            .setOnUpdate(( Vector3 val ) => transform.position = val)
            .setOnComplete(() =>
            {
                this.transform.SetParent(placeholderParent);
                this.transform.SetSiblingIndex(placeholderSibIndex);
            })
            .id;
    }

    internal void ClientStopLeanMovement( bool destroyPlaceholder = false )
    {
        FreePlaceholder();

        if (LeanTween.descr(_LTMovementId) != null)
            LeanTween.cancel(_LTMovementId);
    }

    static internal void FreePlaceholder()
    {
        placeholder.transform.SetParent(PlayerCanvas.singleton.transform);
    }
}
