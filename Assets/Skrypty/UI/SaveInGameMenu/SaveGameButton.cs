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

    void OnClick()
    {
        // TODO: Players can't save when there are cards on board
        // TODO: Players can't save during fights

        string path = SaveGameMenu.pathToSaveIn;
        string fileName = Path.GetFileNameWithoutExtension(path);

        if (File.Exists(path))
        {
            // Ask if Player wants to overwrite the File
            SaveOverwriteAlert.Alert();
            return;
        }

        SaveSystem.SaveGame(path);
    }
}
