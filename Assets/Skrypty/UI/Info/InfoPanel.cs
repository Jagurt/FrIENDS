using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Class for handling Info and Alert objects
/// </summary>
public class InfoPanel : MonoBehaviour
{
    [SerializeField] GameObject infoTradeRequest;
    [SerializeField] GameObject info;
    [SerializeField] GameObject alert;
    static Transform infoPanelContent;
    static InfoPanel infoPanel;

    GameObject activeAlert;
    List<string> alerts = new List<string>();

    private void Start()
    {
        infoPanel = this;
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
    /// <summary>
    /// Called when players gets Trade Request.
    /// </summary>
    static internal void ReceiveTradeRequestInfo( PlayerInGame requestingTradePIG )
    {
        // Preventing creation of duplicate requests from same player.
        foreach (InfoTradeRequest tradeRequest in infoPanelContent.GetComponentsInChildren<InfoTradeRequest>())
            if (tradeRequest.RequestingTradePIG == requestingTradePIG)
                return;

        GameObject infoTR = Instantiate(infoPanel.infoTradeRequest);
        infoTR.GetComponent<InfoTradeRequest>().Initialize(requestingTradePIG);
        ReceiveInfo(infoTR);
    }
    /// <summary>
    /// Called when requested for trade player danied request.
    /// </summary>
    static internal void ReceiveTradeReqDenial( PlayerInGame requestedTradePIG )
    {
        GameObject info = Instantiate(infoPanel.info);
        info.GetComponent<Info>().Initialize(requestedTradePIG.NickName + " refused to trade!");
        ReceiveInfo(info);
    }
    /// <summary>
    /// Creating alert and adding it to queue.
    /// </summary>
    /// <param name="text"></param>
    static internal void Alert( string text )
    {
        infoPanel.alerts.Add(text);
        // If queue is not started, start queue.
        if (!infoPanel.activeAlert)
            InitializeAlert();
    }
    /// <summary>
    /// Starting alert that is at top of the queue.
    /// </summary>
    static void InitializeAlert()
    {
        infoPanel.activeAlert = Instantiate(infoPanel.alert, PlayerInGame.playerCanvas.transform);
        infoPanel.activeAlert.GetComponent<Alert>().Initialize(infoPanel.alerts[0]);
    }
    /// <summary>
    /// Called by alert from top of the queue when it is destroyed.
    /// Removes it from queue and initializes next one.
    /// </summary>
    static internal void InitializeNextAlert()
    {
        infoPanel.alerts.RemoveAt(0);
        if (infoPanel.alerts.Count > 0)
            InitializeAlert();
    }
}
