using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary> Class for placeholders in choicePanel </summary>
public class ChoicePlaceholder : MonoBehaviour, IPointerClickHandler, IPointerExitHandler
{
    [SerializeField] internal GameObject heldObject;

    public void OnPointerClick( PointerEventData eventData )
    {
        // On left mouse click, select held object.
        if (eventData.button == PointerEventData.InputButton.Left)
            ChoicePanel.Choose(heldObject);
        // On different, call click on held player stats object.
        else if (heldObject.GetComponent<PlayerStats>())
            heldObject.GetComponent<PlayerStats>().OnPointerClick(eventData);
    }

    public void OnPointerExit( PointerEventData eventData )
    {
        // If held object is Player Stats object, call its event
        if (heldObject.GetComponent<PlayerStats>())
            heldObject.GetComponent<PlayerStats>().OnPointerExit(eventData);
    }

    internal void Initialize( ChoicePanel choicePanel, GameObject objectToHold, Transform heldObjectContainer )
    {
        Debug.Log("Initializating Placeholder For - " + objectToHold);

        // Preventing cards in choicePanel from being dragged
        if (objectToHold.GetComponent<Draggable>())
            objectToHold.GetComponent<Draggable>().enabled = false;

        objectToHold.GetComponent<Image>().raycastTarget = false;

        // Set placeholder values and hierarchy
        this.transform.SetParent(heldObjectContainer);
        heldObject = objectToHold;
        objectToHold.transform.SetParent(this.transform);
    }

    IEnumerator Initialization()
    {
        yield return new WaitForEndOfFrame();
        ((RectTransform)this.transform).sizeDelta = ((RectTransform)heldObject.transform).sizeDelta;
        ((RectTransform)heldObject.transform).position.Set(0, 0, 0);
    }
}
