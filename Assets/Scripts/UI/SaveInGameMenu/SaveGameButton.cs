using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SaveGameButton : MonoBehaviour
{
    Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(delegate { OnClick(); });
    }

    /// <summary> Saving game under specified file name. </summary>
    void OnClick()
    {
        // Players currently can't save when cards are waiting in queue to be used or during fights.
        if (GameManager.singleton.cardsUsageQueue.Count > 0 || GameManager.singleton.fightInProggres)
        {
            InfoPanel.Alert("Can't save right now!");
            return;
        }

        string path = SaveGameMenu.pathToSaveIn;

        // Ask if Player wants to overwrite the save.
        if (File.Exists(path))
        {
            SaveOverwriteAlert.Alert();
            return;
        }

        SaveSystem.SaveGame(path);
    }
}
