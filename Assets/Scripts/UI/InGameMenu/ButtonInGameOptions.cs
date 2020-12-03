using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonInGameOptions: MonoBehaviour
{
    Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(delegate { OnClick(); } );
    }

    void OnClick()
    {
        Debug.Log("ADD OPENING IN-GAME OPTIONS");
    }
}
