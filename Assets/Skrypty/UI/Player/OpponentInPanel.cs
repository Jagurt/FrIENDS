using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class OpponentInPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] internal PlayerInGame storedPlayer;
    [SerializeField] TextMeshProUGUI NickName;
    [SerializeField] TextMeshProUGUI Level;
    [SerializeField] TextMeshProUGUI OwnedCards;
    [SerializeField] internal bool animating = true;

    public void OnPointerEnter( PointerEventData eventData )
    {
        if (animating)
        {
            LeanTween.scaleX(this.gameObject, 2, .1f);
            LeanTween.scaleY(this.gameObject, 2, .1f);
        }
    }

    public void OnPointerExit( PointerEventData eventData )
    {
        if (animating)
        {
            LeanTween.scaleX(this.gameObject, 1, .1f);
            LeanTween.scaleY(this.gameObject, 1, .1f);
        }
    }

    internal void Initialize( PlayerInGame playerScript )
    {
        storedPlayer = playerScript;

        Transform info = transform.Find("Info");

        if (storedPlayer.Avatar)
            info.Find("Avatar").GetComponent<Image>().sprite = storedPlayer.Avatar;

        Transform stats = info.Find("Stats");

        NickName = stats.Find("NickName").GetComponentInChildren<TextMeshProUGUI>(true);
        Level = stats.Find("Level").GetComponentInChildren<TextMeshProUGUI>(true);
        OwnedCards = stats.Find("OwnedCards").GetComponentInChildren<TextMeshProUGUI>(true);

        NickName.text = storedPlayer.NickName;
    }

    internal void InitializeInChoicePanel( PlayerInGame playerScript )
    {
        Initialize(playerScript);

        Transform EqTransform = transform.Find("Eq");
        if(!EqTransform) EqTransform = transform.Find("EnemyEq");

        GridLayoutGroup grid = EqTransform.GetComponent<GridLayoutGroup>();

        Debug.Log("grid - " + grid);

        EqTransform.GetComponent<Image>().raycastTarget = false;
    }

    internal void UpdateOwnedCards( int numberOfCards, int cardsInHandLimit )
    {
        OwnedCards.text = "Cards: " + numberOfCards +"\\" + cardsInHandLimit;
    }

    internal void UpdateLevel( int level)
    {
        Level.text = "Level: " + level;
    }
}
