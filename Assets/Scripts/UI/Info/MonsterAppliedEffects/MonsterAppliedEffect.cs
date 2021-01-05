using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MonsterAppliedEffect: MonoBehaviour, IPointerClickHandler
{
    TextMeshProUGUI titleTMPro;
    GameObject appliedEffectCard;

    internal void Initialize( string title )
    {
        this.titleTMPro = GetComponentInChildren<TextMeshProUGUI>();
        this.titleTMPro.text = title;
        this.name = title;
    }

    public void OnPointerClick( PointerEventData eventData )
    {
        //TODO Add showing card on right click
        Debug.Log("Add showing card on right click");
    }
}
