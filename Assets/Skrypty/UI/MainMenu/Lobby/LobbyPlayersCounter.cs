using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPlayersCounter : MonoBehaviour
{
    static LobbyPlayersCounter lobbyPlayersCounter;

    static TMPro.TextMeshProUGUI header;
    static TMPro.TextMeshProUGUI counter;

    internal static int numOfLoadedPlayers = 0;

    void Start()
    {
        lobbyPlayersCounter = this;

        counter = transform.Find("Counter").GetComponent<TMPro.TextMeshProUGUI>();
        header = transform.Find("Header").GetComponent<TMPro.TextMeshProUGUI>();
    }

    internal void Initialize()
    {
        numOfLoadedPlayers = 0;
    }

    internal static void UpdatePlayersCounter( short numOfPlayersReady, short numOfPlayersPresent )
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
        LobbyManager.UpdatePlayersCounter();
    }
}
