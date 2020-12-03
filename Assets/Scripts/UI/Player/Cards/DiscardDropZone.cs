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
            PlayerInGame.localPlayerInGame.DiscardCard(new List<GameObject> { eventData.pointerDrag.gameObject });
            //PlayerInGame.localPlayerInGame.CmdDiscardCard(eventData.pointerDrag.GetComponent<Card>().netId);
        }
    }

    public void OnPointerEnter( PointerEventData eventData )
    {

    }

    public void OnPointerExit( PointerEventData eventData )
    {

    }
}
