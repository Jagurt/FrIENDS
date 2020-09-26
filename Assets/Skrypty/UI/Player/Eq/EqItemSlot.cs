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
        localPlayerInGame = PlayerInGame.localPlayerInGame;
    }

    public void OnBeginDrag( PointerEventData eventData )
    {
        if (!heldItem)
        {
            return;
        }

        localPlayerInGame.Unequip(heldItem);
        heldItem.SetActive(true);

        eventData.pointerDrag = heldItem;
        heldItem.GetComponent<Draggable>().OnBeginDrag(eventData);

        heldItem = null;
    }

    override
    public void OnDrop( PointerEventData eventData )
    {
        GameObject card = eventData.pointerDrag;
        EquipmentCard eqCard = card.GetComponent<EquipmentCard>();

        if (eqCard != null)
        {
            if ((eqCard.cardValues as EquipmentValue).eqPart == eqPart)
                eqCard.UseCard();
            else
                card.GetComponent<Draggable>().ReturnToParent();

            Destroy(card.GetComponent<Draggable>().Placeholder);
        }            
    }

    override
    public void OnPointerEnter( PointerEventData eventData )
    {
        //Debug.Log("EqItemSlot.OnPointerEnter(): eventData.pointerDrag - " + eventData.pointerDrag);

        if (eventData.pointerDrag == null)
        {
            return;
        }

        Draggable draggable = eventData.pointerDrag.GetComponent<Draggable>();

        if (!draggable) return;

        EquipmentCard equipmentCard = draggable.GetComponent<EquipmentCard>();

        if (draggable != null && equipmentCard != null && 
            (equipmentCard.cardValues as EquipmentValue).eqPart == eqPart)
        {
            equipmentCard.gameObject.SetActive(false);
            eqImage.sprite = equipmentCard.GetComponentInChildren<Image>().sprite;
        }
    }

    override
    public void OnPointerExit( PointerEventData eventData )
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

    internal void ReceiveEq( GameObject card )
    {
        //Debug.Log("ReceiveEq(GameObject card) card - " + card);

        heldItem = card;
        card.SetActive(false);
        eqImage.sprite = card.GetComponentInChildren<Image>().sprite;
    }

    internal void ReturnEq()
    {
        Debug.Log("ReturnEq()");

        heldItem.transform.SetParent(PlayerInGame.localPlayerInGame.handContent);
        heldItem.gameObject.SetActive(true);
        heldItem.GetComponent<CanvasGroup>().blocksRaycasts = true;
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

    public void OnDrag( PointerEventData eventData ) { }
}
