using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Card : NetworkBehaviour
{
    [SerializeField] internal CardValues cardValues;
    [SerializeField] internal Deck deck;
    protected ServerGameManager serverGameManager;

    void Start()
    {
        deck = cardValues.deck;
        transform.Find("CardDescription").GetComponent<TMPro.TextMeshProUGUI>().text = cardValues.description;
    }

    internal virtual void UseCard()
    {
        throw new NotImplementedException();
    }

    [Server]
    virtual internal IEnumerator EffectOnUse( NetworkInstanceId targetNetId )
    {
        if (CustomNetworkManager.customNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.customNetworkManager.isServerBusy);
        CustomNetworkManager.customNetworkManager.isServerBusy = true;
    }

    virtual protected void Initialize()
    {
        deck = cardValues.deck;
        transform.Find("CardDescription").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardValues.description;
        transform.Find("CardImage").GetComponent<Image>().sprite = cardValues.sprite;
    }
}
