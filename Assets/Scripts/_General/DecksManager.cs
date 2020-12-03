#pragma warning disable CS0618 // Type too old lul

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DecksManager : NetworkBehaviour
{
    //      Lists of cards in each deck     //
    [SerializeField] private List<GameObject> doors = new List<GameObject>();
    [SerializeField] private List<GameObject> treasures = new List<GameObject>();
    [SerializeField] private List<GameObject> helpHands = new List<GameObject>();
    [SerializeField] private List<GameObject> spells = new List<GameObject>();

    //      References to UI transforms     //
    [SerializeField] Transform background;
    [SerializeField] Transform table;
    [SerializeField] Transform doorsDeck;
    [SerializeField] Transform treasuresDeck;
    [SerializeField] Transform helpHandsDeck;
    [SerializeField] Transform spellsDeck;
    [SerializeField] Transform discardedDoorsDeck;
    [SerializeField] Transform discardedtreasuresDeck;
    [SerializeField] Transform discardedHelpHandsDeck;
    [SerializeField] Transform discardedSpellsDeck;

    //      Poperties       //
    public List<GameObject> Doors { get => doors; }
    public List<GameObject> Treasures { get => treasures; }
    public List<GameObject> HelpHands { get => helpHands; }
    public List<GameObject> Spells { get => spells; }


    public Transform DoorsDeck { get => doorsDeck; }
    public Transform TreasuresDeck { get => treasuresDeck; }
    public Transform HelpHandsDeck { get => helpHandsDeck; }
    public Transform SpellsDeck { get => spellsDeck; }
    public Transform DiscardedDoorsDeck { get => discardedDoorsDeck; }
    public Transform DiscardedTreasuresDeck { get => discardedtreasuresDeck; }
    public Transform DiscardedHelpHandsDeck { get => discardedHelpHandsDeck; }
    public Transform DiscardedSpellsDeck { get => discardedSpellsDeck; }

    void Start()
    {
        // Setting UI references.
        background = GameObject.Find("PlayerCanvas").transform.Find("Background");

        doorsDeck = background.Find("DoorsDeck");
        treasuresDeck = background.Find("TreasuresDeck");
        helpHandsDeck = background.Find("HelpHandDeck");
        spellsDeck = background.Find("SpellsDeck");
        discardedDoorsDeck = background.Find("DiscardedDoorsDeck");
        discardedtreasuresDeck = background.Find("DiscardedTreasuresDeck");
        discardedHelpHandsDeck = background.Find("DiscardedHelpHandDeck");
        discardedSpellsDeck = background.Find("DiscardedSpellsDeck");
        table = background.Find("Table");

        // Shuffling decks and spawning cards on server.
        if (isServer)
        {
            ShuffleDecks();
            SpawnCards();
        }
    }

    [Server]
    public void ShuffleDecks()
    {
        ShuffleDeck(doors);
        ShuffleDeck(treasures);
        ShuffleDeck(helpHands);
        ShuffleDeck(spells);
    }

    [Server]
    void ShuffleDeck( List<GameObject> deck )
    {
        //Debug.Log("Filling indexes");

        List<int> indexes = new List<int>();
        // List of decks indexes
        for (int i = 0; i < deck.Count; i++)
        {
            indexes.Add(i);
        }

        // Debug.Log("Shuffling");
        // Getting 2 random indexes from indexes list and swapping objects under those indexes in deck list.
        // After that used indexes are deleted from their list to prevent swapping already swapped cards
        for (int i = 0; i < deck.Count / 2; i++)
        {
            int random1 = Random.Range(0, indexes.Count);
            int random2 = Random.Range(0, indexes.Count);

            //Debug.Log("Iteration = " + i);
            //Debug.Log("Numbers [" + random1 + "] = " + numbers[random1]);
            //Debug.Log("Numbers [" + random2 + "] = " + numbers[random2]);

            // Check if index list has >=1 value left
            if (indexes[random1] == indexes[random2])
            {
                // This checks appears to be useless but it prevents accesing indexes[0] when its already been destroyed.
                if (indexes[random1] != null) 
                {
                    indexes.RemoveAt(random1);
                    continue;
                }
                else
                    break;
            }

            // Swapping cards under randomized indexes.
            GameObject temp = deck[indexes[random1]];
            deck[indexes[random1]] = deck[indexes[random2]];
            deck[indexes[random2]] = temp;

            // Removing first higher and then lower index.
            if (random1 > random2)
            {
                indexes.RemoveAt(random1);
                indexes.RemoveAt(random2);
            }
            else
            {
                indexes.RemoveAt(random2);
                indexes.RemoveAt(random1);
            }
        }
    }

    [Server]
    void ReshuffleDeck( List<GameObject> deck, List<GameObject> discardedDeck )
    {
        deck = new List<GameObject>(discardedDeck);
        discardedDeck.Clear();
        ShuffleDeck(deck);
    }

    [Server]
    void SpawnCards()
    {
        for (int i = 0; i < doors.Capacity; i++)
            NetworkServer.Spawn(Instantiate(doors[i]));
        for (int i = 0; i < treasures.Count; i++)
            NetworkServer.Spawn(Instantiate(treasures[i]));
        for (int i = 0; i < helpHands.Count; i++)
            NetworkServer.Spawn(Instantiate(helpHands[i]));
        for (int i = 0; i < spells.Count; i++)
            NetworkServer.Spawn(Instantiate(spells[i]));
    }
    /// <summary>
    /// Used when loading game.
    /// Searching for card with specified name.
    /// </summary>
    /// <returns> Found card. </returns>
    [Server]
    public GameObject GetCardByName( string cardName )
    {

        for (int i = 0; i < doorsDeck.childCount; i++)
        {
            GameObject card = doorsDeck.GetChild(i).gameObject;

            if (card.GetComponent<Card>().cardValues.name == cardName)
                if (card.GetComponent<Card>().cardValues.name == cardName)
                {
                    card.transform.SetAsLastSibling();
                    return card;
                }
        }
        for (int i = 0; i < treasuresDeck.childCount; i++)
        {
            GameObject card = treasuresDeck.GetChild(i).gameObject;

            if (card.GetComponent<Card>().cardValues.name == cardName)
                if (card.GetComponent<Card>().cardValues.name == cardName)
                {
                    card.transform.SetAsLastSibling();
                    return card;
                }
        }
        for (int i = 0; i < spellsDeck.childCount; i++)
        {
            GameObject card = spellsDeck.GetChild(i).gameObject;

            if (card.GetComponent<Card>().cardValues.name == cardName)
            {
                card.transform.SetAsLastSibling();
                return card;
            }
        }
        for (int i = 0; i < helpHandsDeck.childCount; i++)
        {
            GameObject card = helpHandsDeck.GetChild(i).gameObject;

            if (card.GetComponent<Card>().cardValues.name == cardName)
                if (card.GetComponent<Card>().cardValues.name == cardName)
                {
                    card.transform.SetAsLastSibling();
                    return card;
                }
        }

        Debug.LogError("CardNotFound");
        return null;
    }
}
