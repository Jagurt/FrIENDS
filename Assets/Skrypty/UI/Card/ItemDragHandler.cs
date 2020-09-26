using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDragHandler : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    //private Image image;
    private Outline outline;

    private Vector3 startingPosition;
    private RectTransform rectTransform;
    private Vector2 originalRectTransformSize;
    private Vector2 changedRectTransformSize;

    private bool wybrane;

    public float sizeIncreaseScale;

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = new Vector3(Input.mousePosition.x - changedRectTransformSize.x / 1.5f, Input.mousePosition.y - changedRectTransformSize.y / 1.5f);
        
        wybrane = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.localPosition = startingPosition;
        wybrane = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //      WybierzKarte        //
        if (!wybrane)
        {
            wybrane = true;
            
        }
        else
        {

        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //      Powieksz Karte     //
        if (!wybrane)
        {
            rectTransform.sizeDelta = changedRectTransformSize;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!wybrane)
        {
            rectTransform.sizeDelta = originalRectTransformSize;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        outline = GetComponent<Outline>();

        startingPosition = transform.localPosition;
        originalRectTransformSize = rectTransform.sizeDelta;
        changedRectTransformSize = originalRectTransformSize * sizeIncreaseScale;

        wybrane = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
