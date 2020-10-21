using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InGameMenu : MonoBehaviour
{
    internal static InGameMenu inGameMenu;

    internal static void Initialize()
    {
        inGameMenu = PlayerInGame.localPlayerInGame.playerCanvas.transform.Find("InGameMenu").GetComponent<InGameMenu>();
    }

    public static void Activate()
    {
        inGameMenu.gameObject.SetActive(true);
    }

    public static void Deactivate()
    {
        inGameMenu.gameObject.SetActive(false);
    }
}
