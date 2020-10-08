using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReturnButtonInStartGameMenu : MonoBehaviour
{
    Button Button;

    void Start()
    {
        Button = GetComponent<Button>();
        Button.onClick.AddListener(delegate { OnClick(); });
    }

    void OnClick()
    {
        MainMenu.mainMenu.GetComponent<MenuActiveController>().Activate();
        MainMenu.startGameMenu.GetComponent<MenuActiveController>().Deactivate();
    }
}
