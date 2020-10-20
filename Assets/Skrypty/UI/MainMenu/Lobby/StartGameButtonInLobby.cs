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
        CustomNetworkManager.gameLoaded = System.Convert.ToBoolean(LobbyPlayersCounter.numOfLoadedPlayers);
        CustomNetworkManager.playersToConnect = LobbyManager.lobbyManager.connectedPlayers;
        PlayerManager.ServerPrepToStartGame();
        CustomNetworkManager.customNetworkManager.ServerChangeScene("GameScene");
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
