using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Alert : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] AnimationCurve animationCurve;

    private void Start()
    {
        //Initialize();
    }

    internal void Initialize( string text = "Alert")
    {
        GetComponentInChildren<TextMeshProUGUI>().text = text;
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, Vector3.one, 0.3f).setEase(animationCurve).setOnComplete(FadeOut);
    }

    public void OnPointerClick( PointerEventData eventData )
    {
        LeanTween.scale(gameObject, Vector3.zero, 0.1f).setOnComplete(OnComplete);
    }

    void FadeOut()
    {
        LeanTween.scale(gameObject, Vector3.zero, 0.1f).setDelay(1f).setOnComplete(OnComplete);
    }

    void OnComplete()
    {
        InfoPanel.InitializeNextAlert();
        Destroy(this.gameObject);
    }
}
