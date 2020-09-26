using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class LevelCounter : MonoBehaviour
{
    TextMeshProUGUI PlayersLevels;
    TextMeshProUGUI MonstersLevels;

    internal void Initialize()
    {
        PlayersLevels = transform.Find("PlayersLevels").GetComponent<TextMeshProUGUI>();
        MonstersLevels = transform.Find("MonstersLevels").GetComponent<TextMeshProUGUI>();
        this.gameObject.SetActive(false);
    }

    internal void StartFight()
    {
        this.gameObject.SetActive(true);
        UpdateLevels();
    }

    internal void UpdateLevels()
    {
        PlayersLevels.text = ServerGameManager.serverGameManager.fightingPlayersLevel.ToString();
        MonstersLevels.text = ServerGameManager.serverGameManager.fightingMonstersLevel.ToString();
    }

    internal void EndFight()
    {
        this.gameObject.SetActive(false);
    }
}
