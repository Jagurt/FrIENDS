using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Card : NetworkBehaviour
{
    [SerializeField] internal CardValues cardValues;
    internal Deck deck;
    protected ServerGameManager serverGameManager;

    NetworkInstanceId ownerNetId = NetworkInstanceId.Invalid;
    NetworkInstanceId targetNetId = NetworkInstanceId.Invalid;

    internal GameObject confirmUseButton;
    internal GameObject interruptUseButton;
    internal GameObject declineUseButton;

    [SerializeField] short playersConfirmers = 0;
    [SerializeField] short playersDecliners = 0;
    [SerializeField] bool interrupted = false;
    [SerializeField] IEnumerator awaitUseConfirmation;

    void Start()
    {
        Initialize();

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

        confirmUseButton = Instantiate(CardsButtons.confirmUseButton, transform);
        interruptUseButton = Instantiate(CardsButtons.interruptUseButton, transform);
        declineUseButton = Instantiate(CardsButtons.declineUseButton, transform);

        SetActiveCardUseButtons(false);
    }

    internal virtual void UseCard()
    {
        throw new NotImplementedException();
    }

    [Server]
    internal virtual IEnumerator InitializeAwaitUseConfirmation( NetworkInstanceId targetNetId, NetworkInstanceId ownerNetId )
    {
        Debug.Log("InitializeAwaitUseConfirmation() in - " + this.gameObject);

        this.targetNetId = targetNetId;
        this.ownerNetId = ownerNetId;

        if (CustomNetworkManager.customNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.customNetworkManager.isServerBusy);
        CustomNetworkManager.customNetworkManager.isServerBusy = true;

        RpcInitializeAwaitUseConfirmation();

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.customNetworkManager.isServerBusy = false;

        StartCoroutine(awaitUseConfirmation);
    }

    [ClientRpc]
    internal virtual void RpcInitializeAwaitUseConfirmation()
    {
        Debug.Log("RpcInitializeAwaitUseConfirmation() in - " + this.gameObject);

        serverGameManager.StoredCardUsesToConfirm.Add(this.gameObject);
        PlayerInGame.localPlayerInGame.ClientPutCardOnTable(this.netId);

        GetComponent<Draggable>().enabled = false;
    }

    [Server]
    internal virtual IEnumerator AwaitUseConfirmation()
    {
        Debug.Log("AwaitUseConfirmation() in - " + this.gameObject);

        while (serverGameManager.StoredCardUsesToConfirm[0] != this.gameObject)
            yield return new WaitForSecondsRealtime(0.10f);
        CustomNetworkManager.customNetworkManager.isServerBusy = true;

        RpcAwaitUseConfirmation();

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.customNetworkManager.isServerBusy = false;

        for (int i = 0; i < 30; i++)
        {
            Debug.Log("Time for \"" + gameObject + "\" to be auto-used: " + Math.Abs(i - 30) / 10f);

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
    internal void ConfirmUseCard( bool confirm ) // TODO: Make a button 
    {
        if (confirm) playersConfirmers++;
        else playersDecliners++;

        ConfirmationCheck(false);
    }

    [Server]
    internal void InterruptUseCard()
    {
        interrupted = true;
    }

    [Server]
    internal void ConfirmationCheck( bool endOfTime )
    {
        if (playersDecliners > Mathf.Round(serverGameManager.connectedPlayers * 0.51f))
        {
            StopCoroutine(awaitUseConfirmation);
            StartCoroutine(ClientScene.FindLocalObject(ownerNetId).GetComponent<PlayerInGame>().ServerReceiveCard(this.netId, ownerNetId, false, false));
            awaitUseConfirmation = AwaitUseConfirmation(); // Reassigning Coroutine to make it work next time

            playersConfirmers = 0; // Reseting values
            playersDecliners = 0;
        }
        else if (endOfTime || playersConfirmers >= Mathf.Round(serverGameManager.connectedPlayers * 0.51f))
        {
            StopCoroutine(awaitUseConfirmation);
            StartCoroutine(EffectOnUse(targetNetId));
            awaitUseConfirmation = AwaitUseConfirmation();

            playersConfirmers = 0;
            playersDecliners = 0;
        }
    }

    [Server]
    virtual internal IEnumerator EffectOnUse( NetworkInstanceId targetNetId )
    {
        throw new NotImplementedException();
    }

    [ClientRpc]
    void RpcAwaitUseConfirmation()
    {
        SetActiveCardUseButtons(true);
        PlayerInGame.localPlayerInGame.ClientPutCardOnTable(this.netId);
    }

    internal void SetActiveCardUseButtons( bool active )
    {
        if (active)
        {
            confirmUseButton.SetActive(true);
            interruptUseButton.SetActive(true);
            declineUseButton.SetActive(true);
        }
        else
        {
            confirmUseButton.SetActive(false);
            interruptUseButton.SetActive(false);
            declineUseButton.SetActive(false);
        }
    }
}
