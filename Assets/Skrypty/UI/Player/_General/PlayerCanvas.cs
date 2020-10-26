using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCanvas : MonoBehaviour
{
    internal static PlayerCanvas playerCanvas;

    PlayerCanvas()
    {
        playerCanvas = this;
    }
}
