using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 
/// EquipmentValue derives from CardValues class
/// it has all parent class attributes and 1 more, needed for equipment behaviour.
///  </summary>
[CreateAssetMenu(fileName = "New Equipment Value", menuName = "Equipment Value")]
public class EquipmentValue : CardValues
{
    public EqPart eqPart;
}
