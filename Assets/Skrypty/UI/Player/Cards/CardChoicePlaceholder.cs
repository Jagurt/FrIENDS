using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Class for placeholders in choicePanel
/// </summary>
public class CardChoicePlaceholder : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] internal GameObject heldObject;

    public void OnPointerClick(PointerEventData eventData)
    {
        ChoicePanel.Choose(heldObject);
    }

    internal void Initialize( ChoicePanel choicePanel, GameObject objectToHold, Transform heldObjectContainer)
    {
        Debug.Log("Initializating Placeholder For - " + objectToHold);

        // Preventing cards in choicePanel from being dragged
        if (objectToHold.GetComponent<Draggable>())
            objectToHold.GetComponent<Draggable>().enabled = false;
        else
            // Prevent OpponentStatsUI from being animated
            objectToHold.GetComponent<OpponentInPanel>().animating = false; 

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
