using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameMenuButton : MonoBehaviour
{
    Button button;
    GameObject InGameMenu;
    // Start is called before the first frame update
    void Start()
    {
        InGameMenu = transform.parent.Find("InGameMenu").gameObject;
        button = GetComponent<Button>();
        button.onClick.AddListener(delegate { OnClick(); });
    }

    void OnClick()
    {
        InGameMenu.GetComponent<InGameMenu>().OpenInGameMenu();
    }
}
