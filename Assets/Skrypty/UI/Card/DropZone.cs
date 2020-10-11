using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    protected PlayerInGame localPlayerInGame;

    private void Start()
    {
        localPlayerInGame = PlayerInGame.localPlayerInGame;
    }

    virtual
    public void OnDrop(PointerEventData eventData)
    {
        Draggable draggable = eventData.pointerDrag.GetComponent<Draggable>();

        if (draggable != null)
        {
            draggable.parentToReturnTo = this.transform;
        }
    }

    virtual
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

    virtual
    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
        {
            return;
        }

        Draggable draggable = eventData.pointerDrag.GetComponent<Draggable>();

        if (draggable != null && draggable.placeholderParent == this.transform)
            draggable.placeholderParent = draggable.parentToReturnTo;
    }
}
