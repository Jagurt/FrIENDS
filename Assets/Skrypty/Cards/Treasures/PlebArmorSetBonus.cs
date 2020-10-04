using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlebArmorSetBonus : PassiveEffect
{
    internal override void ActivateEffect()
    {
        if (this.transform.parent.parent.GetChild((int)EqPart.Head).GetChild(0).name == "Bucket")
        {
            //  Bonus + 1 do Levela
        }
    }
}
