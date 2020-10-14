using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnloadGameButton : MonoBehaviour
{
    Button Button;

    void Start()
    {
        Button = GetComponent<Button>();
        Button.onClick.AddListener(delegate { OnClick(); });
    }

    void OnClick()
    {
        LobbyPlayersCounter.numOfLoadedPlayers = 0;
        LobbyManager.UpdatePlayersCounter();
        StartCoroutine(LobbyManager.ServerDeactivateLoadHeaders());
    }
}
