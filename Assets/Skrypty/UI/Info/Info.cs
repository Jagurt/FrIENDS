using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Info : MonoBehaviour
{
    protected TextMeshProUGUI message;


    virtual internal void Initialize(string message)
    {
        this.message = transform.Find("Message").GetComponent<TextMeshProUGUI>();
        this.message.text = message;
    }

    virtual internal void Discard()
    {
        Destroy(this.gameObject);
    }
}
