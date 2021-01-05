using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyoutSubmissionPanel : MonoBehaviour
{
    internal static BuyoutSubmissionPanel singleton;

    static TextMeshProUGUI notificationText;
    readonly static string regularText = "Do you really want to buy that goal?";
    readonly static string insufficientText = "You do not have enough money!";

    static Button buttonYes;
    static Button buttonNo;
    static Button buttonOk;

    BuyoutSubmissionPanel()
    {
        if (!singleton)
            singleton = this;
        else
            Debug.LogError("Two \"BuyoutSubmissionPanel\" scripts!");
    }

    static internal void Initialize()
    {
        notificationText = singleton.transform.Find("NotificationText").GetComponent<TextMeshProUGUI>();

        buttonYes = singleton.transform.Find("ButtonYes").GetComponent<Button>();
        buttonNo = singleton.transform.Find("ButtonNo").GetComponent<Button>();
        buttonOk = singleton.transform.Find("ButtonOk").GetComponent<Button>();

        buttonNo.onClick.AddListener(delegate { CloseSubmissionPanel(); });
        buttonOk.onClick.AddListener(delegate { CloseSubmissionPanel(); });
    }

    static void Prompt()
    {
        singleton.gameObject.SetActive(true);

        buttonNo.gameObject.SetActive(false);
        buttonYes.gameObject.SetActive(false);
        buttonOk.gameObject.SetActive(false);

        buttonYes.onClick.RemoveAllListeners();
    }

    static internal void PromptRegular()
    {
        Prompt();

        notificationText.text = regularText;

        buttonYes.gameObject.SetActive(true);
        buttonNo.gameObject.SetActive(true);

        buttonYes.onClick.AddListener(delegate { ButtonYesRegular(); });
    }

    static internal void PromptDefaultGoal()
    {
        Prompt();

        notificationText.text = regularText;

        buttonYes.gameObject.SetActive(true);
        buttonNo.gameObject.SetActive(true);

        buttonYes.onClick.AddListener(delegate { ButtonYesDefault(); });
    }

    static internal void PromptInsufficientMoney()
    {
        Prompt();

        notificationText.text = insufficientText;

        buttonOk.gameObject.SetActive(true);
    }

    static void ButtonYesRegular()
    {
        PlayerInGame.localPlayerInGame.BuyoutGoal();
        CloseSubmissionPanel();
    }

    static void ButtonYesDefault()
    {
        PlayerInGame.localPlayerInGame.BuyLevelUp();
        CloseSubmissionPanel();
    }

    static void CloseSubmissionPanel()
    {
        singleton.gameObject.SetActive(false);
    }
}
