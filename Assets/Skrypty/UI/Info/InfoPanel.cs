using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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
        if (!infoObject.GetComponent<Info>())
            Debug.LogError("InfoPanel received wrong object - " + infoObject);

        infoObject.transform.SetParent(infoPanelContent);
    }

    static internal void ReceiveTradeReqInfo( PlayerInGame requestingTradePIG )
    {
        foreach (InfoTradeRequest tradeRequest in infoPanelContent.GetComponentsInChildren<InfoTradeRequest>())
        {
            if (tradeRequest.RequestingTradePIG == requestingTradePIG)
                return;
        }

        GameObject infoTR = Instantiate(infoPanel.infoTradeRequest);
        infoTR.GetComponent<InfoTradeRequest>().Initialize(requestingTradePIG);
        ReceiveInfo(infoTR);
    }

    static internal void ReceiveTradeReqDenial( PlayerInGame requestingTradePIG )
    {
        GameObject info = Instantiate(infoPanel.info);
        info.GetComponent<Info>().Initialize(requestingTradePIG.NickName + " refused to trade!");
        ReceiveInfo(info);
    }

    static internal void Alert( string text )
    {
        infoPanel.alerts.Add(text);

        if (!infoPanel.activeAlert)
        {
            InitializeAlert();
        }
    }

    static internal void InitializeNextAlert()
    {
        infoPanel.alerts.RemoveAt(0);
        if(infoPanel.alerts.Count > 0)
        {
            InitializeAlert();
        }
    }

    static void InitializeAlert()
    {
        infoPanel.activeAlert = Instantiate(infoPanel.alert, PlayerInGame.localPlayerInGame.playerCanvas.transform);
        infoPanel.activeAlert.GetComponent<Alert>().Initialize(infoPanel.alerts[0]);
    }
}
