using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 
/// Class for holding static references to card buttons prefabs
/// I created these buttons after making several cards and in order to skip adding them
/// to each card manually I do it programatically in main Card class using following prefabs.
/// </summary>
public class CardsAddons: MonoBehaviour
{
    [SerializeField] private GameObject _confirmUseButton;
    [SerializeField] private GameObject _interruptUseButton;
    [SerializeField] private GameObject _interruptUseTimer;
    [SerializeField] private GameObject _declineUseButton;
    [SerializeField] private GameObject _cardLight;
    [SerializeField] private Material _cardBGColor;
    [SerializeField] private Material _cardDescBGColor;

    [SerializeField] private GameObject _treasureCostInfo;
    [SerializeField] private GameObject _equipmentSlotInfo;
    [SerializeField] private GameObject _monsterLevelInfo;

    [SerializeField] private List<Sprite> _cardBGImages = new List<Sprite>();

    static internal GameObject confirmUseButton;
    static internal GameObject interruptUseButton;
    static internal GameObject interruptUseTimer;
    static internal GameObject declineUseButton;
    static internal GameObject cardLight;
    static internal Material cardBGColor;
    static internal Material cardDescBGColor;

    static internal GameObject treasureCostInfoPrefab;
    static internal GameObject equipmentSlotInfoPrefab;
    static internal GameObject monsterLevelInfoPrefab;

    static internal List<Sprite> cardBGImages;

    void Start()
    {
        if (!confirmUseButton)
        {
            confirmUseButton = _confirmUseButton;
            interruptUseButton = _interruptUseButton;
            interruptUseTimer = _interruptUseTimer;
            declineUseButton = _declineUseButton;
            cardLight = _cardLight;
            cardBGColor = _cardBGColor;
            cardDescBGColor = _cardDescBGColor;

            cardBGImages = _cardBGImages;

            treasureCostInfoPrefab = _treasureCostInfo;
            equipmentSlotInfoPrefab = _equipmentSlotInfo;
            monsterLevelInfoPrefab = _monsterLevelInfo;
        }
    }

    static internal Sprite GetRandomCardBG()
    {
        int index = Random.Range(0, cardBGImages.Count);
        return cardBGImages[index];
    }
}
