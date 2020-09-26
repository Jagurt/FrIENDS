using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMainMenuButton : MonoBehaviour
{
    Button Button;

    // Start is called before the first frame update
    void Start()
    {
        Button = GetComponent<Button>();
        Button.onClick.AddListener(delegate { OnClick(); });
    }

    void OnClick()
    {
        MainMenu.mainMenu.GetComponent<MenuActiveController>().Deactivate();
        MainMenu.optionsMenu.GetComponent<MenuActiveController>().Activate();
    }
}
