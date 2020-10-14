using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public bool hasTurn;
    public bool isAlive;
    public short level;
    public string nickName;

    public Color color;

    public List<string> equippedItems;
    public List<string> cardsInHand;
}
