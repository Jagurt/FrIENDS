using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Script for dragging PlayerInLobby object.
/// </summary>
public class PlayerInLobbyDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameObject placeholder = null;

    public void OnBeginDrag( PointerEventData eventData )
    {
        if (!GlobalVariables.IsHost)
            return;

        CreatePlaceholder();
    }

    public void OnDrag( PointerEventData eventData )
    {
        if (!GlobalVariables.IsHost)
            return;

        this.transform.position = eventData.position;

        int newSiblingIndex = placeholder.transform.parent.childCount;
        for (int i = 0; i < placeholder.transform.parent.childCount; i++)
        {
            if (this.transform.position.y > placeholder.transform.parent.GetChild(i).position.y)
            {
                newSiblingIndex = i;

                if (placeholder.transform.GetSiblingIndex() < newSiblingIndex)
                    newSiblingIndex--;

                break;
            }
        }
        placeholder.transform.SetSiblingIndex(newSiblingIndex);
    }

    public void OnEndDrag( PointerEventData eventData )
    {
        if (!GlobalVariables.IsHost)
            return;

        this.transform.SetParent(placeholder.transform.parent);
        this.transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());
        Destroy(placeholder);

        StartCoroutine(PlayerInLobby.ServerUpdateAllPILPositions());
    }

    void CreatePlaceholder()
    {
        placeholder = new GameObject();

        placeholder.transform.SetParent(MainMenu.lobby.Find("LobbyPanel"));
        placeholder.transform.SetSiblingIndex(this.transform.GetSiblingIndex());

        this.transform.SetParent(MainMenu.lobby);

        LayoutElement layoutElement = placeholder.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = this.GetComponent<LayoutElement>().preferredWidth;
        layoutElement.preferredHeight = this.GetComponent<LayoutElement>().preferredHeight;
    }
}
