using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaveLobbyButton : MonoBehaviour
{
    Button Button;

    void Start()
    {
        Button = GetComponent<Button>();
        Button.onClick.AddListener(delegate { OnClick(); });
    }

    void OnClick()
    {
        MainMenu.lobby.GetComponent<MenuActiveController>().Deactivate();
        MainMenu.joinGameMenu.GetComponent<MenuActiveController>().Activate();
        MainMenu.NetworkManager.Disconnect();
    }
}
