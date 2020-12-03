using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartGameMainMenuButton : MonoBehaviour
{
    Button Button;

    void Start()
    {
        Button = GetComponent<Button>();
        Button.onClick.AddListener(delegate { OnClick(); });
    }

    void OnClick()
    {
        MainMenu.mainMenu.GetComponent<MenuActiveController>().Deactivate();
        MainMenu.startGameMenu.GetComponent<MenuActiveController>().Activate();
    }
}
