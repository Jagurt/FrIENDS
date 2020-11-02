﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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

        if (objectToHold.GetComponent<Draggable>())
            objectToHold.GetComponent<Draggable>().enabled = false; // Preventing cards in choicePanel from being dragged
        else
            objectToHold.GetComponent<OpponentInPanel>().animating = false; // Prevent EnemyPanel from being animated

        this.transform.SetParent(heldObjectContainer);
        heldObject = objectToHold;
        objectToHold.transform.SetParent(this.transform);

        // StartCoroutine(Initialization());
    }

    IEnumerator Initialization()
    {
        yield return new WaitForEndOfFrame();
        ((RectTransform)this.transform).sizeDelta = ((RectTransform)heldObject.transform).sizeDelta;
        ((RectTransform)heldObject.transform).position.Set(0, 0, 0);
    }
}
