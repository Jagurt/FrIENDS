using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CardTitle : MonoBehaviour
{
    void Start()
    {
        this.GetComponent<Text>().text = GetComponentInParent<Card>().cardValues.name;
    }
}
