using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class SaveNameInputField : MonoBehaviour
{
    internal static SaveNameInputField saveNameInputField;
    internal static TMP_InputField tMP_InputField;

    internal void Initialize()
    {
        saveNameInputField = this;
        tMP_InputField = GetComponent<TMP_InputField>();
        tMP_InputField.onValueChanged.AddListener(delegate { UpdatePathToSaveIn(); });
    }

    internal static void UpdateInputFieldText()
    {
        tMP_InputField.text = Path.GetFileNameWithoutExtension(SaveGameMenu.pathToSaveIn);
    }

    internal static void UpdatePathToSaveIn()
    {
        SaveGameMenu.pathToSaveIn = SaveSystem.savesFolderPath + tMP_InputField.text + ".json";
    }
}
