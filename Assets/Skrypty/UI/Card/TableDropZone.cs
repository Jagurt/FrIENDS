using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary> Script for handling player interaction with table. </summary>
public class TableDropZone : DropZone
{
    List<GameObject> borrowedCardPlaceholders = new List<GameObject>();
    List<GameObject> borrowedCards = new List<GameObject>();

    internal static TableDropZone tableDropZone;

    private void Start()
    {
        tableDropZone = this;
    }
    /// <summary> Call cards "UseCard" method when card is dropped on table. </summary>
    override
    public void OnDrop(PointerEventData eventData)
    {
        Draggable draggable = eventData.pointerDrag.GetComponent<Draggable>();

        if (!draggable) return;

        draggable.GetComponent<Card>().UseCard();
    }
    /// <summary> Changing cards placeholder parent to be table when cursor enters. </summary>
    override public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        Draggable draggable = eventData.pointerDrag.GetComponent<Draggable>();
        if (draggable != null)
            draggable.placeholderParent = this.transform;
    }
    /// <summary> Returning cards placeholder parent to its former parent on cursor exit. </summary>
    override public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        Draggable draggable = eventData.pointerDrag.GetComponent<Draggable>();
        if (draggable != null && draggable.placeholderParent == this.transform)
            draggable.placeholderParent = draggable.parentToReturnTo;
    }
    /// <summary> 
    /// Finding all monsters on table and returning them. 
    /// Saving them to list to be able to get their back to the table.
    /// </summary>
    internal IEnumerable<GameObject> BorrowMonsterCards()
    {
        var monsters = this.GetComponentsInChildren<MonsterCard>();

        foreach (var monster in monsters)
        {
            GameObject placeholder = new GameObject();
            placeholder.transform.SetParent(this.transform);
            LayoutElement layoutElement = placeholder.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = monster.GetComponent<LayoutElement>().preferredWidth;
            layoutElement.preferredHeight = monster.GetComponent<LayoutElement>().preferredHeight;
            placeholder.transform.SetSiblingIndex(monster.transform.GetSiblingIndex());
            borrowedCardPlaceholders.Add(placeholder);
            borrowedCards.Add(monster.gameObject);

            yield return monster.gameObject;
        }
    }

    internal void ReturnBorrowedCards()
    {
        for (int i = 0; i < borrowedCards.Count; i++)
        {
            borrowedCards[i].transform.SetParent(this.transform);
            borrowedCards[i].transform.SetSiblingIndex(borrowedCardPlaceholders[i].transform.GetSiblingIndex());
            borrowedCards[i].GetComponent<Draggable>().enabled = true;
            Destroy(borrowedCardPlaceholders[i].gameObject);
        }

        borrowedCardPlaceholders.Clear();
        borrowedCards.Clear();
    }
}
