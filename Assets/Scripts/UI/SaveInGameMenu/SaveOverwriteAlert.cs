using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class for alerting player when they try to overwrite existing file.
/// </summary>
public class SaveOverwriteAlert : MonoBehaviour
{
    internal static SaveOverwriteAlert saveOverwriteAlert;

    Button overwriteSaveButtonYes;
    Button overwriteSaveButtonNo;

    private void Awake()
    {
        saveOverwriteAlert = this;

        overwriteSaveButtonYes = transform.Find("OverwriteSaveButtonYes").GetComponent<Button>();
        overwriteSaveButtonNo = transform.Find("OverwriteSaveButtonNo").GetComponent<Button>();

        overwriteSaveButtonYes.onClick.AddListener(delegate { AcceptOverwriteOnClick(); });
        overwriteSaveButtonNo.onClick.AddListener(delegate { DeclineOverwriteOnClick(); });
    }

    internal void Initialize()
    {
        saveOverwriteAlert = this;

        overwriteSaveButtonYes = transform.Find("OverwriteSaveButtonYes").GetComponent<Button>();
        overwriteSaveButtonNo = transform.Find("OverwriteSaveButtonNo").GetComponent<Button>();

        overwriteSaveButtonYes.onClick.AddListener(delegate { AcceptOverwriteOnClick(); });
        overwriteSaveButtonNo.onClick.AddListener(delegate { DeclineOverwriteOnClick(); });
    }

    internal static void Alert()
    {
        saveOverwriteAlert.gameObject.SetActive(true);
    }

    internal static void AcceptOverwriteOnClick()
    {
        SaveSystem.SaveGame(SaveGameMenu.pathToSaveIn);
        saveOverwriteAlert.gameObject.SetActive(false);
    }

    internal static void DeclineOverwriteOnClick()
    {
        SaveGameMenu.pathToSaveIn = SaveGameMenu.defaultPathToSaveIn;
        SaveNameInputField.UpdateInputFieldText();
        saveOverwriteAlert.gameObject.SetActive(false);
    }
}
