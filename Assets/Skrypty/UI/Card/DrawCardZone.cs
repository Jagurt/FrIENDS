using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class DrawCardZone : MonoBehaviour
{
    public Deck deckType;

    /// <summary> Put card under this object in hierarchy. </summary>
    /// <param name="card"></param>
    public void ReceiveCard( Transform card )
    {
        card.SetParent(this.transform, true);
        card.transform.position = Vector3.zero;
    }

    /// <summary> Called on server when drawing card </summary>
    /// <returns> Return last card from deck. </returns>
    public GameObject DrawCard()
    {
        if (this.transform.childCount <= 0)
            Debug.Log("You draw card from empty deck!");

        Card drawnCard = this.transform.GetChild(this.transform.childCount - 1).GetComponent<Card>();
        drawnCard.gameObject.SetActive(true);

        return drawnCard.gameObject;
    }
}
