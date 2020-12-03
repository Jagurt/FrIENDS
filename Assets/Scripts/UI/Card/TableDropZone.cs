#pragma warning disable CS0618 // Typ lub składowa jest przestarzała

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Networking;

/// <summary> Script for handling player interaction with table. </summary>
public class TableDropZone : DropZone
{
    List<GameObject> borrowedCardPlaceholders = new List<GameObject>();
    List<GameObject> borrowedCards = new List<GameObject>();

    internal static TableDropZone singleton;
    internal static Transform cardActiveZone;
    internal static Transform cardQueueZone;
    internal static Transform cardOnwersPlaceholder;
    internal static Transform cardTargetPlaceholder;
    internal static Transform cardServicePlaceholder;

    static GameObject cardOwnerObject;
    static GameObject cardTargetObject;

    [SerializeField] private GameObject cardNoTargetObject;

    private void Start()
    {
        singleton = this;
        cardActiveZone = transform.Find("CardActiveZone");
        cardQueueZone = transform.Find("CardQueueZone");
        cardOnwersPlaceholder = transform.Find("CardServiceZone").Find("OwnerPlaceholder");
        cardTargetPlaceholder = transform.Find("CardServiceZone").Find("TargetPlaceholder");
        cardServicePlaceholder = transform.Find("CardServiceZone").Find("ServicedCardPlaceholder");
    }
    /// <summary> Call cards "UseCard" method when card is dropped on table. </summary>
    override
    public void OnDrop( PointerEventData eventData )
    {
        Draggable draggable = eventData.pointerDrag.GetComponent<Draggable>();

        if (!draggable) return;
        draggable.GetComponent<Card>().UseCard();
    }
    /// <summary> Changing cards placeholder parent to be table when cursor enters. </summary>
    override public void OnPointerEnter( PointerEventData eventData )
    {
        if (eventData.pointerDrag == null)
            return;

        Draggable draggable = eventData.pointerDrag.GetComponent<Draggable>();
        if (draggable != null)
            draggable.placeholderParent = this.transform;
    }
    
    /// <summary> 
    /// Finding all monsters on table and returning them. 
    /// Saving them to list to be able to get their back to the table.
    /// </summary>
    internal IEnumerable<GameObject> BorrowMonsterCards()
    {
        var monsters = cardActiveZone.GetComponentsInChildren<MonsterCard>();

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

    internal void RetrieveBorrowedCards()
    {
        for (int i = 0; i < borrowedCards.Count; i++)
        {
            borrowedCards[i].transform.SetParent(cardActiveZone);
            borrowedCards[i].transform.SetSiblingIndex(borrowedCardPlaceholders[i].transform.GetSiblingIndex());
            borrowedCards[i].GetComponent<Draggable>().enabled = true;
            Destroy(borrowedCardPlaceholders[i].gameObject);
        }

        borrowedCardPlaceholders.Clear();
        borrowedCards.Clear();
    }

    static internal IEnumerator ReceiveServicedCard( GameObject card, NetworkInstanceId ownerNetId, NetworkInstanceId targetNetId )
    {
        //Debug.Log("targetNetId - " + targetNetId);

        card.GetComponent<Draggable>().ClientSlide(cardServicePlaceholder);

        // Check if card has a target
        if (targetNetId != NetworkInstanceId.Invalid)
        {
            GameObject targetGO = ClientScene.FindLocalObject(targetNetId);
            MonoBehaviour targetScript = null;

            // Check if target is player or monster
            if (targetScript = targetGO.GetComponent<MonsterCard>())
            {
                cardTargetObject = Instantiate(targetGO, cardTargetPlaceholder);
                //Debug.Log("cardTargetObject - " + cardTargetObject);
                yield return new WaitForEndOfFrame();
                (cardTargetObject.transform as RectTransform).localPosition = Vector3.zero;
            }
            else if (targetScript = targetGO.GetComponent<PlayerInGame>())
            {
                cardTargetObject = Instantiate((targetScript as PlayerInGame).opponentStatsUIPrefab);
                singleton.StartCoroutine(
                    cardTargetObject.GetComponent<PlayerStats>()
                    .Initialize(targetScript as PlayerInGame));

                cardTargetObject.transform.SetParent(cardTargetPlaceholder, false);
            }
        }
        else
        {
            cardTargetObject = Instantiate(singleton.cardNoTargetObject);
            cardTargetObject.transform.SetParent(cardTargetPlaceholder, false);
        }

        // Get Owner object, copy its Stats UI and place it in Onwer Placeholder
        cardOwnerObject = ClientScene.FindLocalObject(ownerNetId);
        PlayerInGame ownerScript = cardOwnerObject.GetComponent<PlayerInGame>();

        cardOwnerObject = Instantiate(ownerScript.opponentStatsUIPrefab);
        singleton.StartCoroutine(cardOwnerObject.GetComponent<PlayerStats>().Initialize(ownerScript));
        cardOwnerObject.transform.SetParent(cardOnwersPlaceholder, false);

        yield return null;
    }

    static internal void OnCardConfirm()
    {
        Destroy(cardTargetObject);
        Destroy(cardOwnerObject);
    }
}
