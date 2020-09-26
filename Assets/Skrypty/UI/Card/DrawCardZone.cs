using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class DrawCardZone : MonoBehaviour, IPointerClickHandler, IPointerExitHandler
{
    public Deck deckType;

    public void OnPointerClick( PointerEventData eventData )
    {
        // TODO Dobieranie na klik
        switch (deckType)
        {
            case Deck.Doors:
                //gameManager.UseCard(this.transform.GetChild(transform.childCount - 1).GetComponent<Card>());
                //ActivateLastChild();
                break;
            case Deck.Spells:
                break;
            case Deck.Treasures:
                break;
            case Deck.HelpingHand:
                break;
        }
    }

    //private void Update()
    //{
    //    ActivateLastChild();
    //}

    public void OnPointerExit( PointerEventData eventData )
    {
        if (eventData.pointerDrag == null)
        {
            return;
        }

        Draggable draggable = eventData.pointerDrag.GetComponent<Draggable>();
        if (draggable != null && draggable.placeholderParent == this.transform)
        {
            draggable.placeholderParent = draggable.parentToReturnTo;
        }
    }

    public void ReceiveCard( Transform card )
    {
        card.SetParent(this.transform, true);
        card.transform.position = Vector3.zero;
    }

    // Sets cards parent to player who draws it
    public GameObject DrawCard()
    {
        if (this.transform.childCount <= 0)
            Debug.LogError("You draw card from empty deck!");

        Card drawnCard = this.transform.GetChild(this.transform.childCount - 1).GetComponent<Card>();
        drawnCard.gameObject.SetActive(true);

        return drawnCard.gameObject;
    }
}
