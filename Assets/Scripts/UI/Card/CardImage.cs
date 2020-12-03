using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CardImage : MonoBehaviour
{
    void Start()
    {
        this.GetComponent<Image>().sprite = GetComponentInParent<Card>().cardValues.sprite;
    }
}
