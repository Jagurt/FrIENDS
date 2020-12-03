#pragma warning disable CS0618 // Type too old lul

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary> Blueprint card script for creating scripts for specific cards. </summary>
public class Card : NetworkBehaviour
{
    [SerializeField] internal CardValues cardValues;
    internal Deck deck;
    internal int level;
    protected GameManager gameManager;

    [SyncVar] protected NetworkInstanceId ownerNetId = NetworkInstanceId.Invalid;
    [SyncVar] protected NetworkInstanceId targetNetId = NetworkInstanceId.Invalid;

    GameObject confirmUseButton;
    GameObject interruptUseButton;
    GameObject interruptUseTimer;
    GameObject declineUseButton;

    [SerializeField] List<GameObject> playersConfirmers;
    [SerializeField] List<GameObject> playersDecliners;
    [SerializeField] [SyncVar] int interruptTimer = 0;
    [SerializeField] IEnumerator serverAwaitUseConfirmation;

    void Start()
    {
        Initialize();
        playersConfirmers = new List<GameObject>();
        playersDecliners = new List<GameObject>();
    }

    virtual protected void Initialize()
    {
        //(transform as RectTransform).Translate(new Vector3(0, 0, 0));

        deck = cardValues.deck;
        level = cardValues.level;
        gameManager = GameManager.singleton;

        transform.Find("CardDescription").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardValues.description;
        transform.Find("CardImage").GetComponent<Image>().sprite = cardValues.sprite;

        if (isServer)
            serverAwaitUseConfirmation = ServerServiceCard();
        StartCoroutine(ClientWaitInstantiateAddons());
    }

    /// <summary> Creating card buttons programatically to skip doing it manually for each card. </summary>
    IEnumerator ClientWaitInstantiateAddons()
    {
        yield return new WaitUntil(() => CardsAddons.confirmUseButton);

        confirmUseButton = Instantiate(CardsAddons.confirmUseButton, transform);
        interruptUseButton = Instantiate(CardsAddons.interruptUseButton, transform);
        interruptUseTimer = Instantiate(CardsAddons.interruptUseTimer, transform);
        declineUseButton = Instantiate(CardsAddons.declineUseButton, transform);
        GetComponent<Image>().sprite = CardsAddons.GetRandomCardBG();
        GetComponent<Image>().color = CardsAddons.cardBGColor.color;
        transform.Find("CardDescription").GetComponent<Image>().color = CardsAddons.cardDescBGColor.color;
        Instantiate(CardsAddons.cardLight, transform);

        yield return new WaitUntil(() => confirmUseButton);
        ClientSetActiveCardButtons(false);
    }

    /// <summary> Method called locally when card is used. </summary>
    internal virtual void UseCard()
    {
        throw new NotImplementedException();
    }

    [Server]
    internal void ServerUseCardImmediately( NetworkInstanceId targetNetId )
    {
        this.targetNetId = targetNetId;
        StartCoroutine(ServerActivateCard());
        StartCoroutine(EffectOnUse());
    }

    void ClientPutCardInQueue()
    {
        if (PlayerInGame.localPlayerInGame.netId == ownerNetId)
            return;

        StartCoroutine(GetComponent<Draggable>().ClientSlideWithPlaceholder(TableDropZone.cardQueueZone));
    }

    internal IEnumerator ClientPutCardInService()
    {
        yield return new WaitUntil(() => ownerNetId.Value != NetworkInstanceId.Invalid.Value);
        yield return TableDropZone.ReceiveServicedCard(this.gameObject, ownerNetId, targetNetId);
    }

    /// <summary>
    /// Called when card is placed on board.
    /// Cards target and owner is stored, clients are informed that card has entered usage queue.
    /// </summary>
    [Server]
    internal virtual IEnumerator ServerQueueCard( NetworkInstanceId targetNetId, NetworkInstanceId ownerNetId )
    {
        Debug.Log("StartAwaitUseConfirmation() targetNetId - " + targetNetId + ", ownerNetId - " + ownerNetId + " in - " + this.gameObject);

        if (CustomNetManager.singleton.isServerBusy)
            yield return new WaitUntil(() => !CustomNetManager.singleton.isServerBusy);
        CustomNetManager.singleton.isServerBusy = true;

        this.targetNetId = targetNetId;
        yield return new WaitForEndOfFrame();

        this.ownerNetId = ownerNetId;
        yield return new WaitForEndOfFrame();

        RpcQueueCard();

        yield return new WaitForEndOfFrame();
        CustomNetManager.singleton.isServerBusy = false;

        StartCoroutine(serverAwaitUseConfirmation);
    }

    [ClientRpc]
    protected virtual void RpcQueueCard()
    {
        Debug.Log("RpcStartAwaitUseConfirmation() in - " + this.gameObject);

        gameManager.cardsUsageQueue.Add(this.gameObject);
        ClientPutCardInQueue();

        GetComponent<Draggable>().enabled = false;
    }

    /// <summary>
    /// Card starts waiting until it is atop of usage queue.
    /// When atop of queue, card waits for a duration and then is atomatically accepted.
    /// </summary>
    [Server]
    protected virtual IEnumerator ServerServiceCard()
    {
        Debug.Log("AwaitUseConfirmation() in - " + this.gameObject);

        while (gameManager.cardsUsageQueue[0] != this.gameObject || CustomNetManager.singleton.isServerBusy)
            yield return new WaitForSecondsRealtime(0.10f);
        CustomNetManager.singleton.isServerBusy = true;

        yield return new WaitForEndOfFrame();
        interruptTimer = 0;

        yield return new WaitForEndOfFrame();
        RpcServiceCard();

        yield return new WaitForEndOfFrame();
        CustomNetManager.singleton.isServerBusy = false;

        for (; interruptTimer < 30; interruptTimer++)
        {
            //Debug.Log("Time for \"" + gameObject + "\" to be auto-used: " + Math.Abs(interruptTimer - 30) / 10f);
            yield return new WaitForSecondsRealtime(0.10f);
        }

        ServerConfirmationCheck(true);
    }

    [ClientRpc]
    void RpcServiceCard()
    {
        ClientSetActiveCardButtons(true);
        StartCoroutine(ClientPutCardInService());
        StartCoroutine(ClientInitInterruptTimer());
    }

    /// <summary> Updating interrupt timer UI object for clients. </summary>
    [Client]
    IEnumerator ClientInitInterruptTimer()
    {
        while (interruptTimer < 30 && interruptUseTimer.activeInHierarchy)
        {
            //Debug.Log("Updating CardUseTimer in UI - " + (Math.Abs(interruptTimer - 30) / 10f).ToString("0.0"));
            interruptUseTimer.GetComponent<TMPro.TextMeshProUGUI>().text = (Math.Abs(interruptTimer - 30) / 10f).ToString("0.0");
            yield return new WaitForSecondsRealtime(0.05f);
        }
    }

    [Server]
    internal void ServerConfirmCardUsage( bool confirm, GameObject player )
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

        ServerConfirmationCheck(false);
    }

    [Server]
    internal void ServerInterruptCardUsage()
    {
        interruptTimer -= 50;
    }

    /// <summary> 
    /// Check if player confirmers or decliners have reached a treshhold or if the time has run out.
    /// If yes, stop waiting for card acceptance, use card or return to owner, and reset waiting values.
    /// </summary>
    /// <param name="endOfTime"></param>
    [Server]
    void ServerConfirmationCheck( bool endOfTime )
    {
        Debug.Log("Players To Decline Needed > " + gameManager.playersObjects.Count * 0.51f);
        Debug.Log("Players To Confirm Needed >= " + gameManager.playersObjects.Count * 0.5f);

        if (playersDecliners.Count > gameManager.playersObjects.Count * 0.51f)
        {
            StartCoroutine(ServerOnConfirm());

            PlayerInGame cardOwner = ClientScene.FindLocalObject(ownerNetId).GetComponent<PlayerInGame>();
            StartCoroutine(cardOwner.ServerReceiveCard(this.netId, false, false));
        }
        else if (endOfTime || playersConfirmers.Count >= gameManager.playersObjects.Count * 0.5f)
        {
            StartCoroutine(ServerOnConfirm());
            StartCoroutine(EffectOnUse());
        }
    }

    [Server]
    internal IEnumerator ServerActivateCard()
    {
        if (CustomNetManager.singleton.isServerBusy)
            yield return new WaitUntil(() => !CustomNetManager.singleton.isServerBusy);
        CustomNetManager.singleton.isServerBusy = true;

        RpcPutCardInActive();

        yield return new WaitForEndOfFrame();
        CustomNetManager.singleton.isServerBusy = false;
    }

    [ClientRpc]
    internal void RpcPutCardInActive()
    {
        StartCoroutine(GetComponent<Draggable>().ClientSlideWithPlaceholder(TableDropZone.cardActiveZone));
    }

    [Server]
    IEnumerator ServerOnConfirm()
    {
        // Stopping waiting to confirm.
        StopCoroutine(serverAwaitUseConfirmation);
        // Reassigning Coroutine to make it work next time.
        serverAwaitUseConfirmation = ServerServiceCard();
        // Reseting confirmation values.
        playersConfirmers.Clear();
        playersDecliners.Clear();
        // Removing confirmed card from queue.
        gameManager.cardsUsageQueue.Remove(this.gameObject);


        if (CustomNetManager.singleton.isServerBusy)
            yield return new WaitUntil(() => !CustomNetManager.singleton.isServerBusy);
        CustomNetManager.singleton.isServerBusy = true;

        RpcOnConfirm();

        yield return new WaitForEndOfFrame();
        CustomNetManager.singleton.isServerBusy = false;
    }

    void RpcOnConfirm()
    {
        ClientSetActiveCardButtons(false);
        TableDropZone.OnCardConfirm();
    }

    [Server]
    virtual internal IEnumerator EffectOnUse()
    {
        if (CustomNetManager.singleton.isServerBusy)
            yield return new WaitUntil(() => !CustomNetManager.singleton.isServerBusy);
        CustomNetManager.singleton.isServerBusy = true;

        RpcPutCardInActive();
        yield return new WaitForSecondsRealtime(1f);

        CustomNetManager.singleton.isServerBusy = false;
    }

    /// <summary> Activation of card buttons UI. </summary>
    /// <param name="active"></param>
    [Client]
    internal void ClientSetActiveCardButtons( bool active )
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

    [ClientRpc]
    virtual protected void RpcPlayAnimation()
    {
        throw new NotImplementedException();
    }

    internal NetworkInstanceId GetNetId()
    {
        return this.netId;
    }

    /// <summary> Used in saving game. </summary>
    internal string GetCardData()
    {
        return cardValues.name;
    }
}
