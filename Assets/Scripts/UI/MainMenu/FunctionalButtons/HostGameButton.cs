﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HostGameButton : MonoBehaviour
{
    Button Button;

    void Start()
    {
        Button = GetComponent<Button>();
        Button.onClick.AddListener(delegate { OnClick(); });
    }

    void OnClick()
    {
        MainMenu.startGameMenu.GetComponent<MenuActiveController>().Deactivate();
        MainMenu.lobby.GetComponent<MenuActiveController>().Activate();
        LobbyManager.Initialize();
        CustomNetManager.singleton.StartHosting();
    }
}
