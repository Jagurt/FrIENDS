﻿#pragma warning disable CS0618 // Type too old lul

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
    protected ServerGameManager serverGameManager;

    protected NetworkInstanceId ownerNetId = NetworkInstanceId.Invalid;
    protected NetworkInstanceId targetNetId = NetworkInstanceId.Invalid;

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
        deck = cardValues.deck;
        transform.Find("CardDescription").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardValues.description;
        transform.Find("CardImage").GetComponent<Image>().sprite = cardValues.sprite;
        serverGameManager = ServerGameManager.serverGameManager;

        if (isServer)
            serverAwaitUseConfirmation = ServerAwaitUseConfirmation();
        StartCoroutine(ClientWaitInstantiateButtons());
    }

    /// <summary> Creating card buttons programatically to avoid doing it manually for each card. </summary>
    IEnumerator ClientWaitInstantiateButtons()
    {
        yield return new WaitUntil(() => CardsButtons.confirmUseButton);

        confirmUseButton = Instantiate(CardsButtons.confirmUseButton, transform);
        interruptUseButton = Instantiate(CardsButtons.interruptUseButton, transform);
        interruptUseTimer = Instantiate(CardsButtons.interruptUseTimer, transform);
        declineUseButton = Instantiate(CardsButtons.declineUseButton, transform);

        ClientSetActiveCardButtons(false);
    }

    /// <summary> Method called locally when card is used. </summary>
    internal virtual void UseCard()
    {
        throw new NotImplementedException();
    }

    [Server]
    internal void ServerUseCardImmediately( NetworkInstanceId targetNetId)
    {
        this.targetNetId = targetNetId;
        StartCoroutine(ServerPutCardOnTable());
        StartCoroutine(EffectOnUse());
    }

    [Server]
    internal IEnumerator ServerPutCardOnTable()
    {
        if (CustomNetworkManager.customNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.customNetworkManager.isServerBusy);
        CustomNetworkManager.customNetworkManager.isServerBusy = true;

        RpcPutCardOnTable();

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.customNetworkManager.isServerBusy = false;
    }
    [ClientRpc]
    internal void RpcPutCardOnTable()
    {
        ClientPutCardOnTable();
    }
    [Client]
    internal void ClientPutCardOnTable()
    {
        transform.SetParent(TableDropZone.tableDropZone.transform);
    }

    /// <summary>
    /// Called when card is placed on board.
    /// Cards target and owner is stored, clients are informed that card has entered usage queue.
    /// </summary>
    /// <param name="targetNetId"></param>
    /// <param name="ownerNetId"></param>
    /// <returns></returns>
    [Server]
    internal virtual IEnumerator ServerStartAwaitUseConfirmation( NetworkInstanceId targetNetId, NetworkInstanceId ownerNetId )
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

        StartCoroutine(serverAwaitUseConfirmation);
    }

    [ClientRpc]
    internal virtual void RpcStartAwaitUseConfirmation()
    {
        Debug.Log("RpcStartAwaitUseConfirmation() in - " + this.gameObject);

        serverGameManager.cardsUsageQueue.Add(this.gameObject);
        ClientPutCardOnTable();

        GetComponent<Draggable>().enabled = false;
    }
    
    /// <summary>
    /// Card starts waiting until it is atop of usage queue.
    /// When atop of queue, card waits for a duration and then is atomatically accepted.
    /// </summary>
    [Server]
    internal virtual IEnumerator ServerAwaitUseConfirmation()
    {
        Debug.Log("AwaitUseConfirmation() in - " + this.gameObject);

        while (serverGameManager.cardsUsageQueue[0] != this.gameObject)
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

        ServerConfirmationCheck(true);
    }

    [ClientRpc]
    void RpcAwaitUseConfirmation()
    {
        ClientSetActiveCardButtons(true);
        ClientPutCardOnTable();
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
    internal void ServerConfirmationCheck( bool endOfTime )
    {
        Debug.Log("Players To Decline Needed > " + serverGameManager.playersObjects.Count * 0.51f);
        Debug.Log("Players To Confirm Needed >= " + serverGameManager.playersObjects.Count * 0.5f);

        if (playersDecliners.Count > serverGameManager.playersObjects.Count * 0.51f)
        {
            StopCoroutine(serverAwaitUseConfirmation);
            PlayerInGame cardOwner = ClientScene.FindLocalObject(ownerNetId).GetComponent<PlayerInGame>();
            StartCoroutine(cardOwner.ServerReceiveCard(this.netId, false, false));
            // Reassigning Coroutine to make it work next time
            serverAwaitUseConfirmation = ServerAwaitUseConfirmation();

            // Reseting values
            playersConfirmers.Clear(); 
            playersDecliners.Clear();

            serverGameManager.cardsUsageQueue.Remove(this.gameObject);
        }
        else if (endOfTime || playersConfirmers.Count >= serverGameManager.playersObjects.Count * 0.5f)
        {
            StopCoroutine(serverAwaitUseConfirmation);
            StartCoroutine(EffectOnUse());
            serverAwaitUseConfirmation = ServerAwaitUseConfirmation();

            playersConfirmers.Clear();
            playersDecliners.Clear();

            serverGameManager.cardsUsageQueue.Remove(this.gameObject);
        }
    }

    [Server]
    virtual internal IEnumerator EffectOnUse()
    {
        throw new NotImplementedException();
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
