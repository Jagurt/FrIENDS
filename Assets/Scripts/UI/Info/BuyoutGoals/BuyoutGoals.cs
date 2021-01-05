using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuyoutGoals : MonoBehaviour, IPointerClickHandler
{
    static BuyoutGoals singleton;

    static TextMeshProUGUI titleTMPro;
    static RectTransform titleRectTransform;
    static TextMeshProUGUI costTMPro;
    readonly static string defaultText = "Level up!";
    static int defaultPrice = 1000;

    [SerializeField] private GameObject goalPrefab;
    static private Transform goalsListContent;

    static int currentPrice = 1000;
    public static int CurrentPrice { get => currentPrice; }
    static string currentTitle = "Level up!";
    public static string CurrentTitle { get => currentTitle; }

    BuyoutGoals()
    {
        if (!singleton)
            singleton = this;
        else
            Debug.LogError("Two \"BuyoutGoals\" scripts!");
    }

    private void Start()
    {
        BuyoutSubmissionPanel.Initialize();

        goalsListContent = transform.parent.Find("ListOfGoals").GetComponent<ScrollableContentScaler>().GetContentTransform();

        titleRectTransform = transform.Find("Title") as RectTransform;
        titleTMPro = titleRectTransform.GetComponent<TextMeshProUGUI>();

        costTMPro = transform.Find("Cost").GetComponent<TextMeshProUGUI>();

        AddGoalToList(defaultText, defaultPrice);
    }

    internal static void AddGoalToList( GameObject buyoutGoalCard )
    {
        BuyoutGoalCard buyGCard = buyoutGoalCard.GetComponent<BuyoutGoalCard>();
        string newGoalTitle = buyGCard.cardValues.name;
        int buyoutPrice = buyGCard.buyoutPrice;

        AddGoalToList(newGoalTitle, buyoutPrice);
    }

    static void AddGoalToList( string newGoalTitle, int buyoutPrice)
    {
        singleton.StartCoroutine(UpdateTitle(newGoalTitle, buyoutPrice));

        GameObject goalGO = Instantiate(singleton.goalPrefab, goalsListContent);
        goalGO.GetComponent<BuyoutGoal>().Initialize(newGoalTitle, buyoutPrice);
    }

    internal static IEnumerator RemoveGoalFromList( GameObject buyoutGoalCard )
    {
        string goalTitle = buyoutGoalCard.GetComponent<Card>().cardValues.name;
        if (goalsListContent.Find(goalTitle))
            Destroy(goalsListContent.Find(goalTitle).gameObject);

        yield return new WaitForEndOfFrame();

        if (goalsListContent.childCount <= 0)
            SetTitleToDefault();
        else
        {
            BuyoutGoal lastGoal = goalsListContent.GetChild(goalsListContent.childCount - 1).GetComponent<BuyoutGoal>();
            singleton.StartCoroutine(UpdateTitle(lastGoal.gameObject.name, lastGoal.buyoutPrice));
        }
    }

    public void OnPointerClick( PointerEventData eventData )
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            PromptBuyoutSubmission();
        }
    }

    internal static IEnumerator UpdateTitle( string newTitle, int buyoutPrice )
    {
        currentTitle= newTitle;
        titleTMPro.text = newTitle;

        currentPrice = buyoutPrice;
        costTMPro.text = buyoutPrice.ToString();

        yield return new WaitForEndOfFrame();

        float preferredHeight = titleTMPro.preferredHeight + 10f;
        titleRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);
    }

    static void SetTitleToDefault()
    {
        singleton.StartCoroutine(UpdateTitle(defaultText, defaultPrice));
    }

    static void PromptBuyoutSubmission()
    {
        if(PlayerInGame.localPlayerInGame.Money < currentPrice)
            BuyoutSubmissionPanel.PromptInsufficientMoney();
        else if(titleTMPro.text == defaultText)
            BuyoutSubmissionPanel.PromptDefaultGoal();
        else
            BuyoutSubmissionPanel.PromptRegular();
    }
}
