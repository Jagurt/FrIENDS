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
        textMeshPro = acceptTradeButton.GetComponentInChildren<TextMeshProUGUI>();
    }
    /// <summary>
    /// Changin appearance of button based on acceptance state.
    /// </summary>
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
    /// <summary>
    /// Called when players add/remove cards for trading or at start of new trade
    /// </summary>
    internal static void ResetButton() 
    {
        accepted = false;
        textMeshPro.text = "Accept";
        textMeshPro.color = Color.white;
    }
}
