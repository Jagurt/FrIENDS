using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LeanUIAppearing : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] RectTransform appearingRectTransform;
    [SerializeField] PointerEventData.InputButton clickInputButton;
    [SerializeField] bool isDisablingOnHide = false;
    [SerializeField] bool isVisible = false;
    [SerializeField] float animationSpeed = 0.1f;

    int _LTappearing;
    int _LTdisappearing;

    private void Start()
    {
        //if (!isVisible)
        //{
        //    appearingRectTransform.sizeDelta.Scale(Vector2.zero);
        //    appearingRectTransform.gameObject.SetActive(false);
        //}
        //else
        //{
        //    appearingRectTransform.gameObject.SetActive(true);
        //    appearingRectTransform.sizeDelta.Scale(Vector2.one);
        //}
    }

    private void Awake()
    {
        if (!isVisible)
        {
            appearingRectTransform.sizeDelta.Scale(Vector2.zero);
            appearingRectTransform.gameObject.SetActive(false);
        }
        else
        {
            appearingRectTransform.gameObject.SetActive(true);
            appearingRectTransform.sizeDelta.Scale(Vector2.one);
        }
    }

    public void OnPointerClick( PointerEventData eventData )
    {
        if (eventData.button == clickInputButton)
        {
            if (isVisible) Disappear();
            else Appear();
        }
    }

    void Disappear()
    {
        if (LeanTween.descr(_LTappearing) != null)
            LeanTween.cancel(_LTappearing);

        isVisible = false;

        _LTdisappearing = LeanTween.scale(appearingRectTransform, Vector3.zero, animationSpeed)
            .setEase(LeanTweenType.clamp)
            .setOnComplete(() =>
            {
                if (isDisablingOnHide) appearingRectTransform.gameObject.SetActive(false);
            })
            .id;
    }

    void Appear()
    {
        if (LeanTween.descr(_LTdisappearing) != null)
            LeanTween.cancel(_LTdisappearing);

        isVisible = true;

        if (isDisablingOnHide)
            appearingRectTransform.gameObject.SetActive(true);

        _LTappearing = LeanTween.scale(appearingRectTransform, Vector3.one, animationSpeed)
            .setEase(LeanTweenType.clamp)
            .id;
    }

    internal void DisableAppering()
    {
        Disappear();
        LeanTween.descr(_LTdisappearing).setOnComplete(() => enabled = false);
    }
}
