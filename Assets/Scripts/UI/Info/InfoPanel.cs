using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary> Class for handling Info and Alert objects </summary>
public class InfoPanel : MonoBehaviour
{
    static InfoPanel singleton;

    [SerializeField] GameObject infoTradeRequest;
    [SerializeField] GameObject info;
    [SerializeField] GameObject alert;
    static Transform infoPanelContent;

    GameObject activeAlert;
    List<string> alerts = new List<string>();

    private void Start()
    {
        singleton = this;
        infoPanelContent = transform.Find("Content");
    }
    static internal void ReceiveInfo( GameObject infoObject )
    {
        // If objects doesn't have Info script.
        if (!infoObject.GetComponent<Info>())
        {
            Debug.LogError("InfoPanel received wrong object - " + infoObject);
            return;
        }
        infoObject.transform.SetParent(infoPanelContent);
    }

    /// <summary> Called when players gets Trade Request. </summary>
    static internal void ReceiveTradeRequestInfo( PlayerInGame requestingTradePIG )
    {
        // Preventing creation of duplicate requests from same player.
        foreach (InfoTradeRequest tradeRequest in infoPanelContent.GetComponentsInChildren<InfoTradeRequest>())
            if (tradeRequest.RequestingTradePIG == requestingTradePIG)
                return;

        GameObject infoTR = Instantiate(singleton.infoTradeRequest);
        infoTR.GetComponent<InfoTradeRequest>().Initialize(requestingTradePIG);
        ReceiveInfo(infoTR);
    }

    /// <summary> Called when requested for trade player danied request. </summary>
    static internal void ReceiveTradeReqDenial( PlayerInGame requestedTradePIG )
    {
        GameObject info = Instantiate(singleton.info);
        info.GetComponent<Info>().Initialize(requestedTradePIG.nickName + " refused to trade!");
        ReceiveInfo(info);
    }

    static internal void ReceiveCardUsageInfo(string usingName, string cardName, string targetName )
    {
        GameObject info = Instantiate(singleton.info);

        if (usingName == "")
            cardName = "Used " + cardName;
        else
            cardName = " used " + cardName;

        if (targetName != "")
            targetName = " on " + targetName + ".";
        else
            targetName = ".";

        string message = usingName + cardName + targetName;

        info.GetComponent<Info>().Initialize(message);
        ReceiveInfo(info);
    }

    /// <summary> Creating alert and adding it to queue. </summary>
    /// <param name="text"></param>
    static internal void Alert( string text )
    {
        singleton.alerts.Add(text);
        // If queue is not started, start queue.
        if (!singleton.activeAlert)
            InitializeAlert();
    }

    /// <summary> Starting alert that is at top of the queue. </summary>
    static void InitializeAlert()
    {
        singleton.activeAlert = Instantiate(singleton.alert, PlayerInGame.playerCanvas.transform);
        singleton.activeAlert.GetComponent<Alert>().Initialize(singleton.alerts[0]);
    }

    /// <summary>
    /// Called by alert from top of the queue when it is destroyed.
    /// Removes it from queue and initializes next one.
    /// </summary>
    static internal void InitializeNextAlert()
    {
        singleton.alerts.RemoveAt(0);
        if (singleton.alerts.Count > 0)
            InitializeAlert();
    }

    static internal void AlertCannotUseCard()
    {
        Alert("Cannot use this card now!");
    }
}
