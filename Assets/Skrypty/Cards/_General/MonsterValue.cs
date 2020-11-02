using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 
/// MonsterValue derives from CardValues class
/// it has all parent class attributes and some more, needed for fighting mechanic.
///  </summary>
[CreateAssetMenu(fileName = "New Monster Value", menuName = "Monster Value")]
public class MonsterValue : CardValues
{
    public short treasuresCount = 0;
    public short levelsToGrant = 0;
}
