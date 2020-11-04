using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary> Class for making animated Alert objects. </summary>
public class Alert : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] AnimationCurve animationCurve;

    internal void Initialize( string text = "Alert")
    {
        GetComponentInChildren<TextMeshProUGUI>().text = text;
        // Setting size of object to be 0.
        transform.localScale = Vector3.zero;
        // Making appearing animation via increasing object from 0 size.
        LeanTween.scale(gameObject, Vector3.one, 0.3f).setEase(animationCurve).setOnComplete(FadeOut);
    }
    // Closing alert OnClick.
    public void OnPointerClick( PointerEventData eventData )
    {
        LeanTween.scale(gameObject, Vector3.zero, 0.1f).setOnComplete(OnComplete);
    }
    // Closing alert after delay.
    void FadeOut()
    {
        LeanTween.scale(gameObject, Vector3.zero, 0.1f).setDelay(1f).setOnComplete(OnComplete);
    }
    // Calling showing next alert in queue and destroying this one.
    void OnComplete()
    {
        InfoPanel.InitializeNextAlert();
        Destroy(this.gameObject);
    }
}
