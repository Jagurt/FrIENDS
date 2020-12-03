using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Networking;

public class EqItemSlot : DropZone, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler
{
    public EqPart eqPart;
    [SerializeField] internal Sprite placeholderImage;
    internal Image eqImage;
    [SerializeField] internal GameObject heldItem;

    private void Start()
    {
        eqImage = GetComponent<Image>();
    }
    /// <summary> When heldItem has begun being dragged from item slot. </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag( PointerEventData eventData )
    {
        if (!heldItem)
            return;

        PlayerInGame.localPlayerInGame.Unequip(heldItem);

        // Call OnBeginDrag for held item card.
        eventData.pointerDrag = heldItem;
        heldItem.GetComponent<Draggable>().OnBeginDrag(eventData);

        // Set held item to be null.
        heldItem = null;
    }

    override public void OnDrop( PointerEventData eventData )
    {
        GameObject card = eventData.pointerDrag;
        EquipmentCard eqCardScript = card.GetComponent<EquipmentCard>();

        // If dropped card is in fact an item ( has EquipmentCard script )
        if (eqCardScript != null)
        {
            // If item slot matches eq slot then use card, if not return card.
            if ((eqCardScript.cardValues as EquipmentValue).eqPart == eqPart)
                eqCardScript.UseCard();
            else
                card.GetComponent<Draggable>().ReturnToParent();

            Destroy(card.GetComponent<Draggable>().Placeholder);
        }            
    }

    override public void OnPointerEnter( PointerEventData eventData )
    {
        if (eventData.pointerDrag == null)
            return;

        Draggable draggable = eventData.pointerDrag.GetComponent<Draggable>();

        if (!draggable) return;

        EquipmentCard equipmentCard = draggable.GetComponent<EquipmentCard>();
        
        // Setting item icon in eq slot, if it matches with item slot
        if (draggable != null && equipmentCard != null && 
            (equipmentCard.cardValues as EquipmentValue).eqPart == eqPart)
        {
            eqImage.sprite = equipmentCard.transform.Find("CardImage").GetComponent<Image>().sprite;
        }
    }

    override public void OnPointerExit( PointerEventData eventData )
    {
        if (eventData.pointerDrag == null)
            return;

        Draggable draggable = eventData.pointerDrag.GetComponent<Draggable>();
        if (!draggable)
            return;

        EquipmentCard equipmentCard = draggable.GetComponent<EquipmentCard>();

        if (draggable != null && equipmentCard != null)
        {
            equipmentCard.gameObject.SetActive(true);
            eqImage.sprite = placeholderImage;
        }
    }
    // This has to be here, otherwise other drag events wouldn't work.
    public void OnDrag( PointerEventData eventData ) { }

    internal void ReceiveEq( GameObject card )
    {
        heldItem = card;
        // Putting card away, out of players sight.
        card.transform.SetParent(PlayerInGame.playerCanvas.transform);
        card.transform.position = new Vector3(-100, 0, 0);
        // Setting item icon in eq slot.
        eqImage.sprite = heldItem.transform.Find("CardImage").GetComponent<Image>().sprite;
    }
    internal void ReturnEq()
    {
        Debug.Log("ReturnEq()");

        heldItem.transform.SetParent(PlayerInGame.localPlayerInGame.handContent);
        heldItem.GetComponent<CanvasGroup>().blocksRaycasts = true;
        heldItem.GetComponent<Draggable>().enabled = true;
        heldItem = null;

        eqImage.sprite = placeholderImage;
    }
    internal void ReturnEnemyEq()
    {
        Debug.Log("ReturnEnemyEq()");
        if (heldItem)
            heldItem.gameObject.SetActive(true);
        heldItem = null;
        eqImage.sprite = placeholderImage;
    }
}
