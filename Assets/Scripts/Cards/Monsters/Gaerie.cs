using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Gaerie : MonsterCard
{
    void Start()
    {
        gameManager = GameManager.singleton;
        Initialize();
    }
}
