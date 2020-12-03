using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary> Class for handling dropping card for object on which cards are dropped. </summary>
public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    virtual public void OnDrop(PointerEventData eventData)
    {
        Draggable draggable = eventData.pointerDrag.GetComponent<Draggable>();
        // Changing parent of dropped card to be this transform.
        if (draggable != null)
            draggable.parentToReturnTo = this.transform;
    }

    virtual public void OnPointerEnter(PointerEventData eventData)
    {
        // If nothing is being dragged.
        if (eventData.pointerDrag == null)
            return;

        // Changing dragged cards placeholder parent to this transform.
        Draggable draggable = eventData.pointerDrag.GetComponent<Draggable>();
        if (draggable != null)
            draggable.placeholderParent = this.transform;
    }

    /// <summary> Returning cards placeholder parent to its former parent on cursor exit. </summary>
    virtual public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        // Returning dragged cards placeholder parent to its original parent.
        Draggable draggable = eventData.pointerDrag.GetComponent<Draggable>();
        if (draggable != null && draggable.placeholderParent == this.transform)
            draggable.placeholderParent = draggable.parentToReturnTo;
    }
}
