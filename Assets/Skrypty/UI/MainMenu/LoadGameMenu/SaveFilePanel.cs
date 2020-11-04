using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SaveFilePanel : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    // HardCoded colors values.
    internal static Color NormalColor = new Color(.05098039f, .345098f, .572549f, 1);
    internal static Color HighlightedColor = new Color(.08048778f, .5414634f, .9f, 1);
    internal static Color PressedColor = new Color(.02817072f, .1895122f, .315f, 1);

    bool clicked = false;

    TextMeshProUGUI saveFileName;
    TextMeshProUGUI saveFileDate;
    Image background;

    string saveFilePath;

    internal void Initialize( string saveFilePath, string saveFileName, string saveFileDate )
    {
        this.saveFileName = transform.Find("SaveFileName Text").GetComponent<TextMeshProUGUI>();
        this.saveFileDate = transform.Find("SaveFileDate Text").GetComponent<TextMeshProUGUI>();

        this.saveFilePath = saveFilePath;
        this.saveFileName.text = saveFileName;
        this.saveFileDate.text = saveFileDate;

        background = GetComponent<Image>();
    }

    /// <summary>
    /// Select this panel on click and animate changing color.
    /// </summary>
    public void OnPointerClick( PointerEventData eventData )
    {
        if (!clicked)
        {
            switch (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            {
                case "TitleScene":
                    LoadGameMenu.selectedSavePath = saveFilePath;
                    break;
                case "GameScene":
                    SaveGameMenu.pathToSaveIn = saveFilePath;
                    SaveNameInputField.UpdateInputFieldText();
                    break;
            }
            UnClickAll();
            clicked = true;
            LeanTween.value(gameObject, HighlightedColor, PressedColor, .1f).setOnUpdate(( Color val ) =>
            {
                background.color = val;
            });
        }
        else
        {
            switch (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            {
                case "TitleScene":
                    LoadGameMenu.selectedSavePath = null;
                    break;
                case "GameScene":
                    SaveGameMenu.pathToSaveIn = SaveGameMenu.defaultPathToSaveIn;
                    SaveNameInputField.UpdateInputFieldText();
                    break;
            }
            clicked = false;
            LeanTween.value(gameObject, background.color, HighlightedColor, .1f).setOnUpdate(( Color val ) =>
            {
                background.color = val;
            });
        }
    }
    /// <summary>
    /// Animate highlighting when cursor enters panel.
    /// </summary>
    public void OnPointerEnter( PointerEventData eventData )
    {
        if (!clicked)
            LeanTween.value(this.gameObject, background.color, HighlightedColor, .1f).setOnUpdate(( Color val ) =>
            {
                background.color = val;
            });
    }
    /// <summary>
    /// Animate ending highlighting when cursor exits panel.
    /// </summary>
    public void OnPointerExit( PointerEventData eventData )
    {
        if (!clicked)
            LeanTween.value(this.gameObject, background.color, NormalColor, .1f).setOnUpdate(( Color val ) =>
            {
                background.color = val;
            });
    }

    void UnClick()
    {
        clicked = false;
        LeanTween.value(this.gameObject, background.color, NormalColor, .1f).setOnUpdate(( Color val ) =>
        {
            background.color = val;
        });
    }

    static void UnClickAll()
    {
        SaveFilePanel[] saveFilePanels = FindObjectsOfType<SaveFilePanel>();

        foreach (var item in saveFilePanels)
            item.UnClick();
    }
}
