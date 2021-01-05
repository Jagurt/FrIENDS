using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class BuyoutGoal : MonoBehaviour, IPointerClickHandler
{
    TextMeshProUGUI titleTMPro;
    internal int buyoutPrice = 0;
    
    internal void Initialize(string title, int buyoutPrice)
    {
        this.titleTMPro = GetComponentInChildren<TextMeshProUGUI>();
        this.titleTMPro.text = title;
        this.name = title;
        this.buyoutPrice = buyoutPrice;
    }

    public void OnPointerClick( PointerEventData eventData )
    {
        StartCoroutine(BuyoutGoals.UpdateTitle(titleTMPro.text, buyoutPrice));
    }
}
