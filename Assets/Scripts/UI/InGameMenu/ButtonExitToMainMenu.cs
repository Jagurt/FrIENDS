using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonExitToMainMenu : MonoBehaviour
{
    Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(delegate { OnClick(); });
    }

    void OnClick()
    {
        CustomNetManager.singleton.Disconnect();
        CustomNetManager.singleton.ServerChangeScene("TitleScene");
    }
}
