using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HelpButton : MonoBehaviour
{
    TextMeshProUGUI textMesh;
    Button Button;
    int helpers = 0;
    bool helping = false;

    private void Start()
    {
        textMesh = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        Button = GetComponent<Button>();
        Button.onClick.AddListener(delegate { OnClick(); });
        gameObject.SetActive(false);
    }

    internal void Initialize()
    {
        helpers = 0;

        gameObject.SetActive(true);
        if (ServerGameManager.serverGameManager.fightingPlayerNetId == PlayerInGame.localPlayerInGame.netId)
            textMesh.text = "Request Help (0)";
        else
            textMesh.text = "Offer Help";
    }

    internal void UpdateHelpers( bool helping )
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

    internal void ActivateButton()
    {
        gameObject.SetActive(true);
        GetComponent<Image>().raycastTarget = true;
        Initialize();
        helping = false;
    }

    internal void DeactivateButton()
    {
        helping = false;
        gameObject.SetActive(false);
    }
}
