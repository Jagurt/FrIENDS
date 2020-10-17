#pragma warning disable CS0618 // Typ lub składowa jest przestarzała

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class PILLoadHeader : NetworkBehaviour
{
    TextMeshProUGUI TMPro;

    void Start()
    {
        TMPro = transform.Find("TextMeshPro Text").GetComponent<TextMeshProUGUI>();
        GetComponent<Image>().color = LobbyManager.Colors[transform.GetSiblingIndex()];
        gameObject.SetActive(false);
    }

    internal void Initialize(string name)
    {
        Debug.Log("RpcInitialize.name - " + name);

        gameObject.SetActive(true);
        TMPro.text = name;
    }
}
