#pragma warning disable CS0618 // Type too old lul

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

    GameObject confirmUseButton;
    GameObject interruptUseButton;
    GameObject interruptUseTimer;
    GameObject declineUseButton;

    [SerializeField] List<GameObject> playersConfirmers;
    [SerializeField] List<GameObject> playersDecliners;
    [SerializeField] [SyncVar] int interruptTimer = 0;
    [SerializeField] IEnumerator awaitUseConfirmation;

    void Start()
    {
        Initialize();

        playersConfirmers = new List<GameObject>();
        playersDecliners = new List<GameObject>();

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
        interruptUseTimer = Instantiate(CardsButtons.interruptUseTimer, transform);
        declineUseButton = Instantiate(CardsButtons.declineUseButton, transform);

        ClientSetActiveCardUseButtons(false);
    }

    internal virtual void UseCard()
    {
        throw new NotImplementedException();
    }

    [Server]
    internal virtual IEnumerator StartAwaitUseConfirmation( NetworkInstanceId targetNetId, NetworkInstanceId ownerNetId )
    {
        Debug.Log("StartAwaitUseConfirmation() in - " + this.gameObject);

        this.targetNetId = targetNetId;
        this.ownerNetId = ownerNetId;

        if (CustomNetworkManager.customNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.customNetworkManager.isServerBusy);
        CustomNetworkManager.customNetworkManager.isServerBusy = true;

        RpcStartAwaitUseConfirmation();

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.customNetworkManager.isServerBusy = false;

        StartCoroutine(awaitUseConfirmation);
    }

    [ClientRpc]
    internal virtual void RpcStartAwaitUseConfirmation()
    {
        Debug.Log("RpcStartAwaitUseConfirmation() in - " + this.gameObject);

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

        yield return new WaitForEndOfFrame();
        RpcAwaitUseConfirmation();

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.customNetworkManager.isServerBusy = false;

        for (interruptTimer = 0; interruptTimer < 30; interruptTimer++)
        {
            //Debug.Log("Time for \"" + gameObject + "\" to be auto-used: " + Math.Abs(interruptTimer - 30) / 10f);

            yield return new WaitForSecondsRealtime(0.10f);
        }

        ConfirmationCheck(true);
    }

    [ClientRpc]
    void RpcAwaitUseConfirmation()
    {
        ClientSetActiveCardUseButtons(true);
        PlayerInGame.localPlayerInGame.ClientPutCardOnTable(this.netId);
        StartCoroutine(ClientInterruptUseTimerUI());
    }

    [Client]
    IEnumerator ClientInterruptUseTimerUI()
    {
        while (interruptTimer < 30 && interruptUseTimer.activeInHierarchy)
        {
            //Debug.Log("Updating CardUseTimer in UI - " + (Math.Abs(interruptTimer - 30) / 10f).ToString("0.0"));
            interruptUseTimer.GetComponent<TMPro.TextMeshProUGUI>().text = (Math.Abs(interruptTimer - 30) / 10f).ToString("0.0");
            yield return new WaitForSecondsRealtime(0.05f);
        }
    }

    [Server]
    internal void ConfirmUseCard( bool confirm, GameObject player )
    {
        if (confirm && !playersConfirmers.Contains(player))
        {
            playersConfirmers.Add(player);
            playersDecliners.Remove(player);
        }
        else if (!playersDecliners.Contains(player))
        {
            playersDecliners.Add(player);
            playersConfirmers.Remove(player);
        }

        ConfirmationCheck(false);
    }

    [Server]
    internal void InterruptUseCard()
    {
        interruptTimer -= 50;
    }

    [Server]
    internal void ConfirmationCheck( bool endOfTime )
    {
        Debug.Log("Players To Decline Needed > " + serverGameManager.playersObjects.Count * 0.51f);
        Debug.Log("Players To Confirm Needed >= " + serverGameManager.playersObjects.Count * 0.51f);

        if (playersDecliners.Count > serverGameManager.playersObjects.Count * 0.51f)
        {
            StopCoroutine(awaitUseConfirmation);
            PlayerInGame cardOwner = ClientScene.FindLocalObject(ownerNetId).GetComponent<PlayerInGame>();
            StartCoroutine(cardOwner.ServerReceiveCard(this.netId, false, false));
            awaitUseConfirmation = AwaitUseConfirmation(); // Reassigning Coroutine to make it work next time

            playersConfirmers.Clear(); // Reseting values
            playersDecliners.Clear();
        }
        else if (endOfTime || playersConfirmers.Count >= serverGameManager.playersObjects.Count * 0.51f)
        {
            StopCoroutine(awaitUseConfirmation);
            StartCoroutine(EffectOnUse(targetNetId));
            awaitUseConfirmation = AwaitUseConfirmation();

            playersConfirmers.Clear();
            playersDecliners.Clear();
        }
    }

    [Server]
    virtual internal IEnumerator EffectOnUse( NetworkInstanceId targetNetId )
    {
        throw new NotImplementedException();
    }

    [Client]
    internal void ClientSetActiveCardUseButtons( bool active )
    {
        if (active)
        {
            confirmUseButton.SetActive(true);
            interruptUseButton.SetActive(true);
            declineUseButton.SetActive(true);
            interruptUseTimer.SetActive(true);
        }
        else
        {
            confirmUseButton.SetActive(false);
            interruptUseButton.SetActive(false);
            declineUseButton.SetActive(false);
            interruptUseTimer.SetActive(false);
        }
    }

    internal NetworkInstanceId GetNetId()
    {
        return this.netId;
    }

    internal string GetCardData()
    {
        return cardValues.name;
    }
}
