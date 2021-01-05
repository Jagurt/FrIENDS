using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DiscardDropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public void OnDrop( PointerEventData eventData )
    {
        Draggable draggable = eventData.pointerDrag.GetComponent<Draggable>();

        if (draggable != null)
        {
            Draggable.FreePlaceholder();
            PlayerInGame.SellCard(eventData.pointerDrag.gameObject);
        }
    }

    public void OnPointerEnter( PointerEventData eventData )
    {

    }

    public void OnPointerExit( PointerEventData eventData )
    {

    }
}
