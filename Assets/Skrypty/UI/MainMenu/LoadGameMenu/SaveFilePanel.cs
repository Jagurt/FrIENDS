using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;

public class SaveFilePanel : MonoBehaviour, IPointerClickHandler
{
    TextMeshProUGUI saveFileName;
    TextMeshProUGUI saveFileDate;

    string saveFilePath;

    internal void Initialize( string saveFilePath, string saveFileName, string saveFileDate )
    {
        this.saveFileName = transform.Find("SaveFileName Text").GetComponent<TextMeshProUGUI>();
        this.saveFileDate = transform.Find("SaveFileDate Text").GetComponent<TextMeshProUGUI>();

        this.saveFilePath = saveFilePath;
        this.saveFileName.text = saveFileName;
        this.saveFileDate.text = saveFileDate;
    }

    public void OnPointerClick( PointerEventData eventData )
    {
        LoadGameMenu.selectedSavePath = saveFilePath;
        // TODO : Animate Selection
    }
}
