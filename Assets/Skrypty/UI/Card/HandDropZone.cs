using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandDropZone : DropZone
{
    private void Start()
    {
        localPlayerInGame = PlayerInGame.localPlayerInGame;
    }

    override
    public void OnDrop(PointerEventData eventData)
    {
        Draggable draggable = eventData.pointerDrag.GetComponent<Draggable>();

        if (draggable != null)
        {
            draggable.parentToReturnTo = this.transform;
        }
    }

    override
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        Draggable draggable = eventData.pointerDrag.GetComponent<Draggable>();
        if (draggable != null)
        {
            draggable.placeholderParent = this.transform;
        }
    }

    override
    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        Draggable draggable = eventData.pointerDrag.GetComponent<Draggable>();

        if (draggable != null && draggable.placeholderParent == this.transform)
            draggable.placeholderParent = draggable.parentToReturnTo;
    }
}
