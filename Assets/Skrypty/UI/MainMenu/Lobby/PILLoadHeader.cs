#pragma warning disable CS0618 // Typ lub składowa jest przestarzała

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PILLoadHeader : MonoBehaviour
{
    void Start()
    {
        GetComponent<Image>().color = LobbyManager.Colors[transform.GetSiblingIndex()];
        gameObject.SetActive(false);
    }

    
}
