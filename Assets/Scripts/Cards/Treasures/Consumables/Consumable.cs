using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consumable : Effect
{
    GameObject treasureCostGO;

    private void Start()
    {
        gameManager = GameManager.singleton;
        Initialize();
    }

    protected override IEnumerator ClientWaitInstantiateAddons()
    {
        yield return base.ClientWaitInstantiateAddons();

        treasureCostGO = Instantiate(CardsAddons.treasureCostInfoPrefab, transform);
        treasureCostGO.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = (cardValues as TreasureValue).cost.ToString();
    }

}
