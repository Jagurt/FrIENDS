using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPlayersCounter : MonoBehaviour
{
    internal static LobbyPlayersCounter lobbyPlayersCounter;

    static TMPro.TextMeshProUGUI header;
    static TMPro.TextMeshProUGUI counter;

    internal static int numOfLoadedPlayers = 0;

    void Start()
    {
        Initialize();
    }

    internal void Initialize()
    {
        numOfLoadedPlayers = 0;

        lobbyPlayersCounter = this;

        counter = transform.Find("Counter").GetComponent<TMPro.TextMeshProUGUI>();
        header = transform.Find("Header").GetComponent<TMPro.TextMeshProUGUI>();
    }

    internal static void UpdatePlayersCounter( int numOfPlayersReady, int numOfPlayersPresent )
    {
        counter.text = numOfPlayersReady + " / " + numOfPlayersPresent;

        if (numOfLoadedPlayers != 0)
        {
            header.text = "Ready/Connected/Loaded";
            counter.text += " / " + numOfLoadedPlayers;
        }
        else
            header.text = "Ready/Connected";
    }

    static internal void OnLoadGame( int numOfLoadedPlayers )
    {
        LobbyPlayersCounter.numOfLoadedPlayers = numOfLoadedPlayers;
        LobbyManager.lobbyManager.StartCoroutine(LobbyManager.ServerUpdatePlayersCounter());
    }
}
