using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class for handling cards description and automatically scrolling through it 
/// when its too big to fit on card.
/// </summary>
public class CardDescription : MonoBehaviour
{
    [SerializeField] float speed;
    RectTransform text;

    float startingPosition;

    void Start()
    {
        text = (RectTransform)transform.Find("Text");
        startingPosition = text.localPosition.y;

        StartCoroutine(ScrollDownCoroutine());
    }

    IEnumerator ScrollDownCoroutine()
    {
        yield return new WaitForEndOfFrame();
        ScrollDown();
    }
    // Scrolling text down to the end using LeanTween library.
    void ScrollDown()
    {
        if(text.sizeDelta.y > ((RectTransform)transform).sizeDelta.y)
            LeanTween.moveLocalY(text.gameObject, text.localPosition.y + text.sizeDelta.y - ((RectTransform)transform).sizeDelta.y, 1.65f)
            .setDelay(1f).setOnComplete(ScrollUp);
    }
    // Scrolling text back up to beginning using LeanTween library.
    void ScrollUp()
    {
        LeanTween.moveLocalY(text.gameObject, startingPosition, 1.65f)
            .setDelay(1f).setOnComplete(ScrollDown);
    }
}
