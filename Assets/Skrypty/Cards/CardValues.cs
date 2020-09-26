using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Card Value", menuName = "Card Value")]
public class CardValues : ScriptableObject
{
    public Sprite sprite;
    public Sprite SpriteRewers;

    public Deck deck;

    public new string name;
    public string description;
    public short level;
}
