using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SavePlayerData
{
    public bool hasTurn;
    public bool isPlayerAlive;
    public int level;

    public List<GameObject> equippedItems;
    public List<GameObject> cardsInHand;
}
