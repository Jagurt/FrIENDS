using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InGameMenu : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick( PointerEventData eventData )
    {
        this.gameObject.SetActive(false);
    }

    public void OpenInGameMenu()
    {
        this.gameObject.SetActive(true);
    }
}
