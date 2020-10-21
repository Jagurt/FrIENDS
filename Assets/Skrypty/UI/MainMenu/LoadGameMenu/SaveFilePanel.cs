using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SaveFilePanel : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    internal static Color NormalColor = new Color(.05098039f, .345098f, .572549f, 1);
    internal static Color HighlightedColor = new Color(.08048778f, .5414634f, .9f, 1);
    internal static Color PressedColor = new Color(.02817072f, .1895122f, .315f, 1);

    bool clicked = false;

    TextMeshProUGUI saveFileName;
    TextMeshProUGUI saveFileDate;
    Image image;

    string saveFilePath;

    internal void Initialize( string saveFilePath, string saveFileName, string saveFileDate )
    {
        this.saveFileName = transform.Find("SaveFileName Text").GetComponent<TextMeshProUGUI>();
        this.saveFileDate = transform.Find("SaveFileDate Text").GetComponent<TextMeshProUGUI>();

        this.saveFilePath = saveFilePath;
        this.saveFileName.text = saveFileName;
        this.saveFileDate.text = saveFileDate;

        image = GetComponent<Image>();
    }

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
            UnclickAll();
            clicked = true;
            LeanTween.value(gameObject, HighlightedColor, PressedColor, .1f).setOnUpdate(( Color val ) =>
            {
                image.color = val;
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
            LeanTween.value(gameObject, image.color, HighlightedColor, .1f).setOnUpdate(( Color val ) =>
            {
                image.color = val;
            });
        }
    }

    public void OnPointerEnter( PointerEventData eventData )
    {
        if (!clicked)
            LeanTween.value(this.gameObject, image.color, HighlightedColor, .1f).setOnUpdate(( Color val ) =>
            {
                image.color = val;
            });
    }

    public void OnPointerExit( PointerEventData eventData )
    {
        if (!clicked)
            LeanTween.value(this.gameObject, image.color, NormalColor, .1f).setOnUpdate(( Color val ) =>
            {
                image.color = val;
            });
    }

    void UnClick()
    {
        clicked = false;
        LeanTween.value(this.gameObject, image.color, NormalColor, .1f).setOnUpdate(( Color val ) =>
        {
            image.color = val;
        });
    }

    static void UnclickAll()
    {
        SaveFilePanel[] saveFilePanels = FindObjectsOfType<SaveFilePanel>();

        foreach (var item in saveFilePanels)
        {
            item.UnClick();
        }
    }
}
