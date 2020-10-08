using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardsButtons : MonoBehaviour
{
    [SerializeField] private GameObject ConfirmUseButton;
    [SerializeField] private GameObject InterruptUseButton;
    [SerializeField] private GameObject InterruptUseTimer;
    [SerializeField] private GameObject DeclineUseButton;

    static internal GameObject confirmUseButton;
    static internal GameObject interruptUseButton;
    static internal GameObject interruptUseTimer;
    static internal GameObject declineUseButton;

    void Start()
    {
        if (!confirmUseButton)
        {
            confirmUseButton = ConfirmUseButton;
            interruptUseButton = InterruptUseButton;
            interruptUseTimer = InterruptUseTimer;
            declineUseButton = DeclineUseButton;
        }
        else
            Debug.LogError("Cards Buttons Statics set additional times!");
    }
}
