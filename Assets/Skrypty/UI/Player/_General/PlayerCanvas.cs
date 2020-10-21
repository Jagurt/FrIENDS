using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCanvas : MonoBehaviour
{
    internal static PlayerCanvas playerCanvas;
    
    internal static void Initialize(GameObject playerCanvas)
    {
        PlayerCanvas.playerCanvas = playerCanvas.GetComponent<PlayerCanvas>();
    }
}
