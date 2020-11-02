using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 
/// CardValues derives from ScriptableObject class
///  making it basically a struct containing card values
///  objects of which can be convieniently created and its values modified directly in editor.
///  </summary>
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
