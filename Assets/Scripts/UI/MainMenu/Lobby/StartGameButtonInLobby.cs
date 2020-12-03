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
        CustomNetManager.gameLoaded = System.Convert.ToBoolean(LobbyPlayersCounter.numOfLoadedPlayers);
        CustomNetManager.playersToConnect = LobbyManager.lobbyManager.connectedPlayers;
        PlayerManager.ServerPrepToStartGame();
        CustomNetManager.singleton.ServerChangeScene("GameScene");
    }

    internal void EnableStartGameButton()
    {
        if (button)
            button.interactable = true;
    }

    internal void DisableStartGameButton()
    {
        if (button)
            button.interactable = false;
    }
}
