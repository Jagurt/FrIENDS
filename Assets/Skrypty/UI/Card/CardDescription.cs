using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    void ScrollDown()
    {
        if(text.sizeDelta.y > ((RectTransform)transform).sizeDelta.y)
            LeanTween.moveLocalY(text.gameObject, text.localPosition.y + text.sizeDelta.y - ((RectTransform)transform).sizeDelta.y, 1.65f)
            .setDelay(1f).setOnComplete(ScrollUp);
    }

    void ScrollUp()
    {
        LeanTween.moveLocalY(text.gameObject, startingPosition, 1.65f)
            .setDelay(1f).setOnComplete(ScrollDown);
    }
}
