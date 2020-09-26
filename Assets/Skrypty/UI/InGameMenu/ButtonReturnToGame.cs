using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonReturnToGame : MonoBehaviour
{
    Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(delegate { OnClick(); } );
    }

    void OnClick()
    {
        transform.parent.parent.gameObject.SetActive(false);
    }
}
