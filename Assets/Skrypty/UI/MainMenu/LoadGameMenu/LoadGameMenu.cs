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
    internal static string[] savePaths;

    [SerializeField] internal GameObject SaveFilePanel;

    internal static void Initialize()
    {
        if (!loadGameMenu)
            loadGameMenu = MainMenu.loadGameMenu.GetComponent<LoadGameMenu>();
        if (!content)
            content = loadGameMenu.transform.Find("Scroll View").Find("Viewport").Find("Content");

        loadGameMenu.gameObject.SetActive(true);
        selectedSavePath = null;
        SearchForSaves();
    }

    internal static void SearchForSaves(bool refresh = false)
    {
        if (savePaths != null && !refresh)
            return;

        savePaths = Directory.GetFiles(SaveSystem.savesFolderPath);

        foreach (var savePath in savePaths)
        {
            GameObject saveFilePanel = Instantiate(loadGameMenu.SaveFilePanel, content);

            string saveFileName = Path.GetFileNameWithoutExtension(savePath);

            string saveFileDate = File.GetLastWriteTime(savePath).ToString();
            saveFileDate.Replace(' ', '\n');

            saveFilePanel.GetComponent<SaveFilePanel>().Initialize(savePath, saveFileName, saveFileDate);
        }
    }

    internal static void Deactivate()
    {
        loadGameMenu.gameObject.SetActive(false);
    }
}
