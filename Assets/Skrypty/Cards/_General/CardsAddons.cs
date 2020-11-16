using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 
/// Class for holding static references to card buttons prefabs
/// I created these buttons after making several cards and in order to skip adding them
/// to each card manually I do it programatically in main card class using following prefabs.
/// </summary>
public class CardsAddons: MonoBehaviour
{
    [SerializeField] private GameObject ConfirmUseButton;
    [SerializeField] private GameObject InterruptUseButton;
    [SerializeField] private GameObject InterruptUseTimer;
    [SerializeField] private GameObject DeclineUseButton;
    [SerializeField] private GameObject CardLight;

    static internal GameObject confirmUseButton;
    static internal GameObject interruptUseButton;
    static internal GameObject interruptUseTimer;
    static internal GameObject declineUseButton;
    static internal GameObject cardLight;

    void Start()
    {
        if (!confirmUseButton)
        {
            confirmUseButton = ConfirmUseButton;
            interruptUseButton = InterruptUseButton;
            interruptUseTimer = InterruptUseTimer;
            declineUseButton = DeclineUseButton;
            cardLight = CardLight;
        }
    }
}
