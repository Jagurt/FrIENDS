using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour
{
    // List<GameObject> players = new List<GameObject>();
    public Transform table;
    public Transform localPlayerTransform;

    private ServerGameManager serverGameManager;

    public Image doorsDeck;
    public Image treasuresDeck;
    public Image helpHandDeck;
    public Image spellsDeck;
    //public DiscardZone discardedDoorsDeck;
    //public DiscardZone discardedTreasuresDeck;
    //public DiscardZone discardedHelpHandDeck;
    //public DiscardZone discardedSpellsDeck;

    public List<GameObject> cards = new List<GameObject>();
    public CardValues doorValue;
    public CardValues treasureValue;
    public CardValues helpingHandValue;
    public CardValues spellValue;

    //public MonsterCard gaerie;

    public GameObject activePlayer;
    public PlayerInGame fightingPlayer;

    [SerializeField]
    int playerFightLevel;
    [SerializeField]
    int monsterFightLevel;

    int numberOfPlayers;
    int activePlayerIndex;
    [SerializeField]
    int availableTreasures = 0;

    TurnPhase turnPhase = 0;

    public bool fighting;
    bool fought;
    bool isPlayerAlive;
    bool canPlayerTrade;
    bool canPlayerEquip;

    public bool canPlayerDrawDoors;
    public bool canPlayerDrawHelpHand = false;
    public bool canPlayerDrawTreasures = false;
    public bool canPlayerDrawSpells = false;

    // Start is called before the first frame update
    void Start()
    {
        serverGameManager = FindObjectOfType<ServerGameManager>();

        canPlayerDrawDoors = true;
        canPlayerEquip = true;
    }

    // Update is called once per frame
    void Update(){}

    void NextTurn()
    {
        activePlayerIndex = (activePlayerIndex + 1) % numberOfPlayers;
        turnPhase = (int)TurnPhase.Beginning;
    }

    public bool DiceThrow(int modyfikator, int prog)
    {
        if (modyfikator + Random.Range(0, 6) > prog)
        {
            return true;
        }
        return false;
    }

    public void EnableTable()
    {
        table.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    public void DisableTable()
    {
        table.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void DisableDrawDecks()
    {
        doorsDeck.GetComponent<CanvasGroup>().blocksRaycasts = false;
        treasuresDeck.GetComponent<CanvasGroup>().blocksRaycasts = false;
        helpHandDeck.GetComponent<CanvasGroup>().blocksRaycasts = false;
        spellsDeck.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void UpdateDrawDecks()
    {
        if (canPlayerDrawDoors)
            doorsDeck.GetComponent<CanvasGroup>().blocksRaycasts = true;
        else
            doorsDeck.GetComponent<CanvasGroup>().blocksRaycasts = false;

        if (availableTreasures > 0)
            treasuresDeck.GetComponent<CanvasGroup>().blocksRaycasts = true;
        else
            treasuresDeck.GetComponent<CanvasGroup>().blocksRaycasts = false;

        if (canPlayerDrawHelpHand)
            helpHandDeck.GetComponent<CanvasGroup>().blocksRaycasts = true;
        else
            helpHandDeck.GetComponent<CanvasGroup>().blocksRaycasts = false;

        if (canPlayerDrawSpells)
            spellsDeck.GetComponent<CanvasGroup>().blocksRaycasts = true;
        else
            spellsDeck.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    //public void OpenDoor()
    //{
    //    canPlayerDrawDoors = false;
    //    UpdateDrawDecks();

    //    table.GetChild(table.childCount - 1).
    //        GetComponent<Door>().EffectOnOpenDoor();
    //}

    //public void InitiateFight(Transform monster)
    //{
    //    monster.SetParent(table);
    //    fighting = true;
    //    fightingPlayer = activePlayer.GetComponent<Player>();
    //    playerFightLevel = fightingPlayer.level;
    //    monsterFightLevel = monster.GetComponent<MonsterCard>().monsterValue.level;
    //    canPlayerEquip = false;
    //}

    //public void InitiateFight()
    //{
    //    canPlayerDrawDoors = false;
    //    UpdateDrawDecks();
    //    fightingPlayer = activePlayer.GetComponent<Player>();
    //    playerFightLevel = fightingPlayer.level;
    //    fighting = true;
    //    canPlayerEquip = true;
    //}

    public void FightReward(int treasures)
    {
        availableTreasures = treasures;
        UpdateDrawDecks();
        fighting = false;
    }

    //public void LoseLevel(Player player, int levels)
    //{
    //    player.level -= levels;
    //    fighting = false;
    //}

    public void ApplyBuff(Entity entityToApplyOn, int value)
    {
        switch (entityToApplyOn)
        {
            case Entity.Player:
                playerFightLevel += value;
                break;
            case Entity.Monster:
                monsterFightLevel += value;
                break;
            case Entity.Both:
                //MonsterOrPlayerBuffChoice
                break;
        }
    }
}
