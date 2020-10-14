using UnityEngine;
using UnityEngine.UI;

public class StartGameButtonInLobby : MonoBehaviour
{
    private Button button;

    private void Start()
    {
        button = GetComponentInChildren<Button>();
        button.interactable = false;
        button.onClick.AddListener(delegate { OnClick(); });
    }

    void OnClick()
    {
        CustomNetworkManager.customNetworkManager.ServerChangeScene("GameScene");
    }

    internal void EnableStartGameButton()
    {
        button.interactable = true;
    }

    internal void DisableStartGameButton()
    {
        button.interactable = false;
    }
}
