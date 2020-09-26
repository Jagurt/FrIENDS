using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReturnButtonInOptionsMenu : MonoBehaviour
{
    Button Button;

    void Start()
    {
        Button = GetComponent<Button>();
        Button.onClick.AddListener(delegate { OnClick(); });
    }

    void OnClick()
    {
        MainMenu.optionsMenu.GetComponent<MenuActiveController>().Deactivate();
        MainMenu.mainMenu.GetComponent<MenuActiveController>().Activate();
    }
}
