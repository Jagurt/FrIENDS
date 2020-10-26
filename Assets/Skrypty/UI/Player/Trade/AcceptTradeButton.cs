using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AcceptTradeButton : MonoBehaviour
{
    internal static AcceptTradeButton acceptTradeButton;

    Button Button;

    static bool accepted;
    static TextMeshProUGUI textMeshPro;

    AcceptTradeButton()
    {
        acceptTradeButton = this;
    }

    void Awake()
    {
        Button = GetComponent<Button>();
        Button.onClick.AddListener(delegate { OnClick(); });
    }

    internal static void Initialize()
    {
        //acceptTradeButton = TradePanel.tradePanel.GetComponentInChildren<AcceptTradeButton>();
        textMeshPro = acceptTradeButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    void OnClick()
    {
        GetComponentInParent<TradePanel>().AcceptTrade(accepted);

        if (!accepted)
        {
            accepted = true;
            textMeshPro.text = "Confirm";
            textMeshPro.color = Color.yellow;
        }
        else
        {
            textMeshPro.text = "Confirmed";
            textMeshPro.color = Color.green;
        }
    }

    internal static void ResetButton() // To be called when players add/remove cards for trading or at start of new trade
    {
        //if(!textMeshPro)
        //    textMeshPro = GetComponentInChildren<TextMeshProUGUI>();

        accepted = false;
        textMeshPro.text = "Accept";
        textMeshPro.color = Color.white;
    }
}
