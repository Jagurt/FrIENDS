using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AcceptTradeButton : MonoBehaviour
{
    Button Button;
    bool accepted;
    TextMeshProUGUI textMeshPro;

    void Awake()
    {
        Button = GetComponent<Button>();
        Button.onClick.AddListener(delegate { OnClick(); });
        textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
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

    internal void ResetButton() // To be called when players add/remove cards for trading or at start of new trade
    {
        //if(!textMeshPro)
        //    textMeshPro = GetComponentInChildren<TextMeshProUGUI>();

        accepted = false;
        textMeshPro.text = "Accept";
        textMeshPro.color = Color.white;
    }
}
