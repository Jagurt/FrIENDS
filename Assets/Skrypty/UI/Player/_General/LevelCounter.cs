using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class LevelCounter : MonoBehaviour
{
    internal static LevelCounter levelCounter;

    static TextMeshProUGUI PlayersLevels;
    static TextMeshProUGUI MonstersLevels;

    LevelCounter()
    {
        levelCounter = this;
    }

    internal static void Initialize()
    {
        PlayersLevels = levelCounter.transform.Find("PlayersLevels").GetComponent<TextMeshProUGUI>();
        MonstersLevels = levelCounter.transform.Find("MonstersLevels").GetComponent<TextMeshProUGUI>();
        levelCounter.gameObject.SetActive(false);
    }

    internal static void StartFight()
    {
        levelCounter.gameObject.SetActive(true);
        UpdateLevels();
    }

    internal static void UpdateLevels()
    {
        PlayersLevels.text = ServerGameManager.serverGameManager.fightingPlayersLevel.ToString();
        MonstersLevels.text = ServerGameManager.serverGameManager.fightingMonstersLevel.ToString();
    }

    internal static void Deactivate()
    {
        levelCounter.gameObject.SetActive(false);
    }
}
