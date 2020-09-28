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

    NetworkInstanceId ownerNetId = NetworkInstanceId.Invalid;
    NetworkInstanceId targetNetId = NetworkInstanceId.Invalid;
    short playersConfirmers = 0;
    short playersDecliners = 0;
    bool interrupted = false;
    IEnumerator awaitUseConfirmation;

    void Start()
    {
        //Initialize();

        //deck = cardValues.deck;
        //transform.Find("CardDescription").GetComponent<TMPro.TextMeshProUGUI>().text = cardValues.description;
        //transform.Find("CardImage").GetComponent<Image>().sprite = cardValues.sprite;
        //serverGameManager = ServerGameManager.serverGameManager;
        //awaitUseConfirmation = AwaitUseConfirmation();
    }

    virtual protected void Initialize()
    {
        deck = cardValues.deck;
        transform.Find("CardDescription").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardValues.description;
        transform.Find("CardImage").GetComponent<Image>().sprite = cardValues.sprite;
        serverGameManager = ServerGameManager.serverGameManager;
        awaitUseConfirmation = AwaitUseConfirmation();
    }

    internal virtual void UseCard()
    {
        throw new NotImplementedException();
    }

    [Server]
    internal virtual void StartAwaitUseConfirmation( NetworkInstanceId targetNetId, NetworkInstanceId ownerNetId )
    {
        serverGameManager.StoredCardUsesToConfirm.Add(this.gameObject);
        this.targetNetId = targetNetId;
        this.ownerNetId = ownerNetId;

        StartCoroutine(awaitUseConfirmation);
    }

    [Server]
    internal virtual IEnumerator AwaitUseConfirmation()
    {
        while (serverGameManager.StoredCardUsesToConfirm[0] != this.gameObject)
            yield return new WaitForSecondsRealtime(0.10f);

        CustomNetworkManager.customNetworkManager.isServerBusy = true;

        StartCoroutine(PlayerInGame.localPlayerInGame.ServerPutCardOnTable(this.netId));

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.customNetworkManager.isServerBusy = false;

        for (int i = 0; i < 30; i++)
        {
            if (interrupted)
            {
                interrupted = false;
                i -= 50;
            }

            yield return new WaitForSecondsRealtime(0.10f);
        }

        ConfirmationCheck(true);
    }

    [Server]
    internal void ConfirmUse( bool confirm )
    {
        if (confirm) playersConfirmers++;
        else playersDecliners++;

        ConfirmationCheck(false);
    }

    [Server]
    internal void ConfirmationCheck(bool endOfTime)
    {
        if (playersDecliners >= Mathf.Round(serverGameManager.connectedPlayers * 0.51f))
        {
            StartCoroutine(ClientScene.FindLocalObject(ownerNetId).GetComponent<PlayerInGame>().ServerReceiveCard(this.netId, ownerNetId, false, false));
            StopCoroutine(awaitUseConfirmation);
        }
        else if (playersDecliners + playersConfirmers == serverGameManager.connectedPlayers)
        {
            // TODO: In case of draw - random outcome!
            StartCoroutine(EffectOnUse(targetNetId));
            StopCoroutine(awaitUseConfirmation);
        }
        else if (endOfTime || playersConfirmers >= Mathf.Round(serverGameManager.connectedPlayers * 0.51f))
        {
            StartCoroutine(EffectOnUse(targetNetId));
            StopCoroutine(awaitUseConfirmation);
        }
    }

    [Server]
    virtual internal IEnumerator EffectOnUse( NetworkInstanceId targetNetId )
    {
        yield return new WaitForEndOfFrame();

        playersConfirmers = 0;
        playersDecliners = 0;
    }
}
