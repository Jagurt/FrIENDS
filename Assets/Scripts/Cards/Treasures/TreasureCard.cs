#pragma warning disable CS0618 // Type too old lul

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TreasureCard : Card
{
    GameObject treasureCostGO;

    protected override IEnumerator ClientWaitInstantiateAddons()
    {
        yield return base.ClientWaitInstantiateAddons();
        treasureCostGO = Instantiate(CardsAddons.treasureCostInfoPrefab, transform);
        treasureCostGO.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = (cardValues as TreasureValue).cost.ToString();
    }
}
