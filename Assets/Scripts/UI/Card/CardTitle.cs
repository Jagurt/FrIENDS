using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardTitle : MonoBehaviour
{
    void Start()
    {
        this.GetComponentInChildren<TextMeshProUGUI>().text = GetComponentInParent<Card>().cardValues.name;
    }
}
