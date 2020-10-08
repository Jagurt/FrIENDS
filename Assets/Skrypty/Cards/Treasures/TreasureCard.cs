#pragma warning disable CS0618 // Type too old lul

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TreasureCard : Card
{
    public TreasureType treasureType;

    void Start()
    {

    }

    internal override IEnumerator EffectOnUse( NetworkInstanceId targetNetId )
    {
        return base.EffectOnUse(targetNetId);
    }
}
