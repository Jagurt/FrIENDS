using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public bool hasTurn;
    public bool isAlive;
    public int level;
    public string nickName;

    public List<string> equippedItems;
    public List<string> cardsInHand;
}
