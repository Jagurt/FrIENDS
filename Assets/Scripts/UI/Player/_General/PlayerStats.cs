using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PlayerStats : MonoBehaviour, IPointerExitHandler, IPointerClickHandler
{
    int widthOriginal = 100;
    int widthEnlarged = 180;

    [SerializeField] internal PlayerInGame storedPlayer;
    [SerializeField] TextMeshProUGUI NickName;
    [SerializeField] TextMeshProUGUI Level;
    [SerializeField] TextMeshProUGUI OwnedCards;
    [SerializeField] internal bool animating = true;

    bool unfolding = true;
    GameObject eqPanel;

    private void Start()
    {
        eqPanel = transform.Find("Eq").gameObject;

        Transform stats = transform.Find("Stats");
        NickName = stats.Find("NickName").GetComponentInChildren<TextMeshProUGUI>(true);
        Level = stats.Find("Level").GetComponentInChildren<TextMeshProUGUI>(true);
        OwnedCards = stats.Find("OwnedCards").GetComponentInChildren<TextMeshProUGUI>(true);
        StartCoroutine(OnStart());
    }

    IEnumerator OnStart()
    {
        yield return new WaitUntil(() => CardsAddons.cardLight != null);
        Instantiate(CardsAddons.cardLight, transform);
    }

    public void OnPointerClick( PointerEventData eventData )
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            Unfold();
    }

    public void OnPointerExit( PointerEventData eventData )
    {
        // Prevent unintended folding when sitting in Choice Placeholder
        if (transform.parent.GetComponent<ChoicePlaceholder>() && eventData.pointerCurrentRaycast.gameObject == transform.parent.gameObject)
            return;

        Fold();
    }

    /// <summary> Initializing this object in choice panel. </summary>
    internal IEnumerator Initialize( PlayerInGame playerScript )
    {
        yield return new WaitForEndOfFrame();

        storedPlayer = playerScript;

        if (storedPlayer.Avatar)
            transform.Find("Avatar").GetComponent<Image>().sprite = storedPlayer.Avatar;

        Transform stats = transform.Find("Stats");
        NickName = stats.Find("NickName").GetComponentInChildren<TextMeshProUGUI>(true);
        Level = stats.Find("Level").GetComponentInChildren<TextMeshProUGUI>(true);
        OwnedCards = stats.Find("OwnedCards").GetComponentInChildren<TextMeshProUGUI>(true);

        NickName.text = storedPlayer.NickName;
    }

    internal void UpdateOwnedCards( int numberOfCards, int cardsInHandLimit )
    {
        OwnedCards.text = "Cards: " + numberOfCards + "\\" + cardsInHandLimit;
    }

    internal void UpdateLevel( int level )
    {
        Level.text = "Level: " + level;
    }
    /// <summary> Showing more stuff about player. </summary>
    private void Unfold()
    {
        if (!animating)
            return;
        unfolding = true;

        // Longening width
        LeanTween.value(this.gameObject, (this.transform as RectTransform).sizeDelta.x, widthEnlarged, .05f)
                .setOnUpdate(
                    ( float val ) =>
                    //(transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, val)
                    GetComponent<LayoutElement>().preferredWidth = val
                    )
                    // Showing eq
                    .setOnComplete(() =>
                    {
                        if (unfolding)
                            LeanTween.scale(eqPanel, Vector3.one, .1f);
                    });
    }

    /// <summary> Hiding stuff about player. </summary>
    private void Fold()
    {
        if (!animating)
            return;

        unfolding = false;

        // Disappearing of eq
        LeanTween.scale(eqPanel, Vector3.zero, .1f)
            .setOnComplete(
                // Compressing width
                () => LeanTween.value(this.gameObject, (this.transform as RectTransform).sizeDelta.x, widthOriginal, .05f)
                .setOnUpdate(
                    ( float val ) =>
                    GetComponent<LayoutElement>().preferredWidth = val
                    //(transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, val)
                    ));
    }
}
