using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoinGameButton : MonoBehaviour
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
        MainMenu.joinGameMenu.GetComponent<MenuActiveController>().Activate();
    }
}
