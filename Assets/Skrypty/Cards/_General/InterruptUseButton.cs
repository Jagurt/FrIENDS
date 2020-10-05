using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterruptUseButton : MonoBehaviour
{
    Button Button;

    void Start()
    {
        Button = GetComponent<Button>();
        Button.onClick.AddListener(delegate { OnClick(); });
    }

    void OnClick()
    {
        transform.parent.GetComponent<Card>().InterruptUseCard();
    }

}
