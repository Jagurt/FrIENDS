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
        if (LoadGameMenu.selectedSavePath == null)
            return;

        SaveSystem.LoadSaveFile(LoadGameMenu.selectedSavePath);
        LobbyPlayersCounter.OnLoadGame(SaveSystem.loadedSave.playersData.Count);
        LobbyManager.lobbyManager.StartCoroutine(LobbyManager.ServerActivateLoadHeaders());
        LoadGameMenu.Deactivate();
    }
}
