using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoinGameViaIPButton : MonoBehaviour
{
    Button Button;

    void Start()
    {
        Button = GetComponent<Button>();
        Button.onClick.AddListener(delegate { OnClick(); });
    }

    void OnClick()
    {
        MainMenu.joinGameMenu.GetComponent<MenuActiveController>().Deactivate();
        MainMenu.lobby.GetComponent<MenuActiveController>().Activate();
        CustomNetManager.singleton.JoinMatchViaIP();
    }
}
