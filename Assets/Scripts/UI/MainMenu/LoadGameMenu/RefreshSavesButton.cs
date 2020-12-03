using UnityEngine;
using UnityEngine.UI;

public class RefreshSavesButton : MonoBehaviour
{
    Button Button;

    void Start()
    {
        Button = GetComponent<Button>();
        Button.onClick.AddListener(delegate { OnClick(); });
    }

    void OnClick()
    {
        switch (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
        {
            case "TitleScene":
                LoadGameMenu.SearchForSaves(LoadGameMenu.content);
                break;
            case "GameScene":
                LoadGameMenu.SearchForSaves(SaveGameMenu.content);
                SaveGameMenu.pathToSaveIn = SaveGameMenu.defaultPathToSaveIn;
                SaveNameInputField.UpdateInputFieldText();
                break;
        }
    }
}
