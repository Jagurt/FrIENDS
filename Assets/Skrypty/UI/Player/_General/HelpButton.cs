using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HelpButton : MonoBehaviour
{
    internal static HelpButton helpButton;

    Button Button;
    static TextMeshProUGUI textMesh;
    static int helpers = 0;
    static bool helping = false;

    private void Start()
    {
        helpButton = GetComponent<HelpButton>();
        textMesh = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        Button = GetComponent<Button>();
        Button.onClick.AddListener(delegate { OnClick(); });
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Called fighting player when helping player have clicked HelpButton.
    /// </summary>
    internal static void UpdateHelpers( bool helping )
    {
        Debug.Log("Updating Helpers");

        helpers += helping ? 1 : -1;
        textMesh.text = "Request Help (" + helpers + ")";
    }


    void OnClick()
    {
        if (ServerGameManager.serverGameManager.fightingPlayerNetId == PlayerInGame.localPlayerInGame.netId)
            PlayerInGame.localPlayerInGame.RequestHelp();
        else
        {
            if (!helping)
            {
                PlayerInGame.localPlayerInGame.OfferHelp();
                textMesh.text = "Cancel Help";
            }
            else
            {
                PlayerInGame.localPlayerInGame.CancelHelp();
                textMesh.text = "Offer Help";
            }

            helping = !helping;
        }
    }

    internal static void Activate()
    {
        helpers = 0;
        helpButton.gameObject.SetActive(true);

        if (ServerGameManager.serverGameManager.fightingPlayerNetId == PlayerInGame.localPlayerInGame.netId)
            textMesh.text = "Request Help (0)";
        else
            textMesh.text = "Offer Help";

        helpButton.GetComponent<Image>().raycastTarget = true;
        helping = false;
    }

    internal static void DeactivateButton()
    {
        helping = false;
        helpButton.gameObject.SetActive(false);
    }
}
