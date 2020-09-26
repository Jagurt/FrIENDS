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
    [SerializeField] private Transform deckToReturnTo = null;
    [SerializeField] private GameObject placeholder = null;
    [SerializeField] private ServerDecksManager serverDecksManager;
    [SerializeField] private ServerGameManager serverGameManager;

    public GameObject Placeholder { get => placeholder; }

    private void Start()
    {
        serverDecksManager = FindObjectOfType<ServerDecksManager>();
        serverGameManager = FindObjectOfType<ServerGameManager>();
        GetAdopted();
    }

    void GetAdopted()
    {
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

    public void OnBeginDrag( PointerEventData eventData )
    {
        //Debug.Log("OnBeginDrag(): Dragged Object - " + eventData.pointerDrag.gameObject);

        TradeDropZone tradeDropZone;
        if (tradeDropZone = eventData.pointerDrag.gameObject.GetComponentInParent<TradeDropZone>())
            tradeDropZone.OnBeginDrag(eventData.pointerDrag.gameObject);

        CreatePlaceholder();

        GetComponent<CanvasGroup>().blocksRaycasts = false;

        if (parentToReturnTo.GetComponent<DrawCardZone>() != null)
        {
            deckToReturnTo = parentToReturnTo;
        }
        PlayerInGame.localPlayerInGame.EnableTable();
    }

    void CreatePlaceholder()
    {
        placeholder = new GameObject(); // Placeholder as empty gamobject

        placeholder.transform.SetParent(PlayerInGame.localPlayerInGame.handContent); // set its parent to players canvas parent so its not behind any other ui objects
        parentToReturnTo = PlayerInGame.localPlayerInGame.handContent;               // set its parentToReturnTo to handPanel so if it is dropped wrongly it goes to players hand
        placeholderParent = parentToReturnTo;                                               // set its placeholderParent to also handPanel so player can set its position in hand
        placeholder.transform.SetSiblingIndex(this.transform.GetSiblingIndex());

        this.transform.SetParent(PlayerInGame.localPlayerInGame.playerCanvas.transform);

        LayoutElement layoutElement = placeholder.AddComponent<LayoutElement>();            // set placeholders size to be same as held objects
        layoutElement.preferredWidth = this.GetComponent<LayoutElement>().preferredWidth;
        layoutElement.preferredHeight = this.GetComponent<LayoutElement>().preferredHeight;
    }

    public void OnDrag( PointerEventData eventData )
    {
        // Debug.Log("Dragged object - " + eventData.pointerDrag.gameObject);

        this.transform.position = eventData.position;

        if (eventData.pointerCurrentRaycast.gameObject != null && !eventData.pointerCurrentRaycast.gameObject.GetComponent<DropZone>())
            return;

        if (placeholder.transform.parent != placeholderParent)
            placeholder.transform.SetParent(placeholderParent);

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

        placeholder.transform.SetSiblingIndex(newSiblingIndex);
    }

    public void OnEndDrag( PointerEventData eventData )
    {
        if (parentToReturnTo.GetComponent<DrawCardZone>() != null)  // Sprawdzam czy miejsce do którego idzie
        {                                                           // karta ma DrawCardZone skrypt(jest deckiem, więc WRACA do niego), jeśli tak
            ReturnToParent();
            this.transform.position = parentToReturnTo.position;    // to muszę zmienić pozycję karty na pozycję decku
        }
        else if (deckToReturnTo)                                    // jeśli nie wraca do decku to sprawdzam czy karta jest wzięta z decku
        {                                                           // jeśli jest wzięta z decku to muszę aktywować następną kartę w decku do wzięcia
            GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
        else if (                                                   // jeśli karta nie jest wzięta z decku i nie jest dropowana na konkretny DropZone to wraca na poprzednie miejsce
            eventData.pointerCurrentRaycast.gameObject == null ||
            eventData.pointerCurrentRaycast.gameObject.transform == parentToReturnTo ||
            eventData.pointerCurrentRaycast.gameObject.GetComponent<DropZone>() == null)
        {
            ReturnToParent();
        }

        PlayerInGame.localPlayerInGame.DisableTable();

        if (placeholder)
            Destroy(placeholder);
    }

    public void ReturnToParent()
    {
        this.transform.SetParent(parentToReturnTo);

        if (placeholder)
            this.transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());
            
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
}
