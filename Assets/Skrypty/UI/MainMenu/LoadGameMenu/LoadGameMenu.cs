using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadGameMenu : MonoBehaviour
{
    internal static LoadGameMenu loadGameMenu;
    internal static Transform content;

    internal static string selectedSavePath;

    [SerializeField] internal GameObject SaveFilePanel;

    internal static void Initialize()
    {
        loadGameMenu = MainMenu.mainMenu.transform.parent.Find("LoadGameMenu").GetComponent<LoadGameMenu>();
        content = loadGameMenu.transform.Find("Scroll View").Find("Viewport").Find("Content");
    }

    internal static void SearchForSaves( Transform content = null)
    {
        selectedSavePath = null;


        SaveFilePanel[] existingSaveFilePanels = FindObjectsOfType<SaveFilePanel>();

        foreach (var item in existingSaveFilePanels)
        {
            Destroy(item.gameObject);
        }


        if (!content)
            content = LoadGameMenu.content;

        GameObject SaveFilePanel;
        if (loadGameMenu)
            SaveFilePanel = loadGameMenu.SaveFilePanel;
        else
            SaveFilePanel = SaveGameMenu.saveGameMenu.SaveFilePanel;

        string[] savePaths = Directory.GetFiles(SaveSystem.savesFolderPath);

        foreach (var savePath in savePaths)
        {
            GameObject saveFilePanel = Instantiate(SaveFilePanel, content);

            string saveFileName = Path.GetFileNameWithoutExtension(savePath);

            string saveFileDate = File.GetLastWriteTime(savePath).ToString();
            saveFileDate.Replace(' ', '\n');

            saveFilePanel.GetComponent<SaveFilePanel>().Initialize(savePath, saveFileName, saveFileDate);
        }
    }

    internal static void Activate()
    {
        loadGameMenu.gameObject.SetActive(true);
        selectedSavePath = null;
        SearchForSaves();
    }

    internal static void Deactivate()
    {
        loadGameMenu.gameObject.SetActive(false);
    }
}
