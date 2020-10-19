using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadGameButton : MonoBehaviour
{
    Button Button;

    void Start()
    {
        Button = GetComponent<Button>();
        Button.onClick.AddListener(delegate { OnClick(); });
    }

    void OnClick()
    {
        SaveSystem.LoadSaveFile();
        LobbyPlayersCounter.OnLoadGame(SaveSystem.loadedSave.playersData.Count);
        StartCoroutine(LobbyManager.ServerActivateLoadHeaders());
    }
}
