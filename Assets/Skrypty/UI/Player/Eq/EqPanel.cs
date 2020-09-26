using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EqPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter( PointerEventData eventData )
    {
         if ( GetComponent<Image>().raycastTarget ) LeanTween.scaleX(this.gameObject, 2, .1f);
    }

    public void OnPointerExit( PointerEventData eventData )
    {
        if ( GetComponent<Image>().raycastTarget  ) LeanTween.scaleX(this.gameObject, 1, .1f);
    }
}