﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveGameMenu : MonoBehaviour
{
    internal static SaveGameMenu saveGameMenu;
    internal static Transform content;

    internal static string defaultPathToSaveIn;
    internal static string pathToSaveIn;

    [SerializeField] internal GameObject SaveFilePanel;

    internal static void Initialize()
    {
        saveGameMenu = PlayerInGame.localPlayerInGame.playerCanvas.transform.Find("SaveGameMenu").GetComponent<SaveGameMenu>();
        content = saveGameMenu.transform.Find("Scroll View").Find("Viewport").Find("Content");

        SaveNameInputField saveNameInputField = saveGameMenu.transform.Find("SaveName - InputField").GetComponent<SaveNameInputField>();
        saveNameInputField.Initialize();

        SaveOverwriteAlert overwriteSaveAlert = saveGameMenu.transform.Find("OverwriteSaveAlert").GetComponent<SaveOverwriteAlert>();
        overwriteSaveAlert.Initialize();

        defaultPathToSaveIn = pathToSaveIn = SaveSystem.savesFolderPath + "NewSave.json";
    }

    internal static void Activate()
    {
        saveGameMenu.gameObject.SetActive(true);
        LoadGameMenu.SearchForSaves(content);
        pathToSaveIn = defaultPathToSaveIn;
        SaveNameInputField.UpdateInputFieldText();
    }

    internal static void Deactivate()
    {
        saveGameMenu.gameObject.SetActive(false);
    }
}
