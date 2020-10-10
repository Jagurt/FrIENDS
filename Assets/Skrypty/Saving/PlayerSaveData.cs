using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public bool hasTurn;
    public bool isPlayerAlive;
    public int level;

    public List<string> equippedItems;
    public List<string> cardsInHand;
}
