using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReturnButtonInHostGameMenu : MonoBehaviour
{
    Button Button;

    void Start()
    {
        Button = GetComponent<Button>();
        Button.onClick.AddListener(delegate { OnClick(); });
    }

    void OnClick()
    {
        MainMenu.startGameMenu.GetComponent<MenuActiveController>().Activate();
        MainMenu.hostGameMenu.GetComponent<MenuActiveController>().Deactivate();
    }
}
