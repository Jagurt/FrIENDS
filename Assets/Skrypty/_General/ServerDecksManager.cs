using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ServerDecksManager : NetworkBehaviour
{
    [SerializeField] private List<GameObject> doors = new List<GameObject>();
    [SerializeField] private List<GameObject> treasures = new List<GameObject>();
    [SerializeField] private List<GameObject> helpHands = new List<GameObject>();
    [SerializeField] private List<GameObject> spells = new List<GameObject>();

    [SerializeField] Transform serverCanvas;
    [SerializeField] Transform table;

    [SerializeField] Transform doorsDeck;
    [SerializeField] Transform treasuresDeck;
    [SerializeField] Transform helpHandsDeck;
    [SerializeField] Transform spellsDeck;
    [SerializeField] Transform discardedDoorsDeck;
    [SerializeField] Transform discardedtreasuresDeck;
    [SerializeField] Transform discardedHelpHandsDeck;
    [SerializeField] Transform discardedSpellsDeck;
    
    public List<GameObject> Doors { get => doors; }
    public List<GameObject> Treasures { get => treasures; }
    public List<GameObject> HelpHands { get => helpHands; }
    public List<GameObject> Spells { get => spells; }

    public Transform ServerCanvas { get => serverCanvas; }
    public Transform Table { get => table; }

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
        // Ustawiam referencje do poszczególnych decków i stołu
        doorsDeck = serverCanvas.Find("DoorsDeck");
        treasuresDeck = serverCanvas.Find("TreasuresDeck");
        helpHandsDeck = serverCanvas.Find("HelpHandDeck");
        spellsDeck = serverCanvas.Find("SpellsDeck");
        discardedDoorsDeck = serverCanvas.Find("DiscardedDoorsDeck");
        discardedtreasuresDeck = serverCanvas.Find("DiscardedTreasuresDeck");
        discardedHelpHandsDeck = serverCanvas.Find("DiscardedHelpHandDeck");
        discardedSpellsDeck = serverCanvas.Find("DiscardedSpellsDeck");
        table = serverCanvas.Find("Table");

        if (isServer)
        {
            ShuffleDecks();
            SpawnCards();
        }
    }

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
        if (isServer == false)
        {
            Debug.Log("Funkcja ShuffleDeck() wywołana na kliencie, to nie powinno się dziać!");
            // return;
        }

        //Debug.Log("Filling inexes");

        List<int> indexes = new List<int>();
        // Robię poukładaną listę indeksów
        for (int i = 0; i < deck.Count; i++)
        {
            indexes.Add(i);
        }

        // Debug.Log("Shuffling");
        // Losuję 2 liczby z listy indeksów i zamieniam miejscami obiekty z listy pod tymi indeksami
        // potem kasuję wylosowane indeksy z ich listy by nie użyć ich drugi raz
        // wylosowania już zmienionych kart
        for (int i = 0; i < deck.Count / 2; i++)
        {
            int random1 = Random.Range(0, indexes.Count);
            int random2 = Random.Range(0, indexes.Count);

            //Debug.Log("Iteration = " + i);
            //Debug.Log("Numbers [" + random1 + "] = " + numbers[random1]);
            //Debug.Log("Numbers [" + random2 + "] = " + numbers[random2]);

            // Sprawdzenie czy w liście numerów nie został tylko 1 rekord
            if (indexes[random1] == indexes[random2])
            {
                if (indexes[random1] != null)
                {
                    indexes.RemoveAt(random1);
                    continue;
                }
                else
                    break;
            }

            GameObject temp = deck[indexes[random1]];
            deck[indexes[random1]] = deck[indexes[random2]];
            deck[indexes[random2]] = temp;

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

    // Przełożenie kart z odrzuconych do dobrania i przetasowanie
    void ReshuffleDeck( List<GameObject> deck, List<GameObject> discardedDeck )
    {
        if (isServer == false)
        {
            Debug.LogError("Funkcja ReshuffleDeck() wywołana na kliencie, to nie powinno się dziać!");
            return;
        }

        deck = discardedDeck;
        discardedDeck.Clear();

        ShuffleDeck(deck);
    }

    [Server]
    void SpawnCards()
    {
        // Zamieniam Prefab'y kart na GameObject'y kart i potem spawnuje je w grze
        // Tylko serwer może spawnować karty

        for (int i = 0; i < doors.Capacity; i++)
        {
            NetworkServer.Spawn(Instantiate(doors[i]));
        }
        for (int i = 0; i < treasures.Count; i++)
        {
            NetworkServer.Spawn(Instantiate(treasures[i]));
        }
        for (int i = 0; i < helpHands.Count; i++)
        {
            NetworkServer.Spawn(Instantiate(helpHands[i]));
        }
        for (int i = 0; i < spells.Count; i++)
        {
            NetworkServer.Spawn(Instantiate(spells[i]));
        }
    }

    [Server]
    public GameObject SpawnCardByName(string name, PlayerInGame owner = null )
    {
        GameObject cardToSpawn = null;

        foreach (var card in Doors)
            if (card.name == name)
                cardToSpawn = card;

        if (cardToSpawn == null)
            foreach (var card in Treasures)
                if (card.name == name)
                    cardToSpawn = card;

        if (cardToSpawn == null)
            foreach (var card in Spells)
                if (card.name == name)
                    cardToSpawn = card;

        if (cardToSpawn == null)
            foreach (var card in HelpHands)
                if (card.name == name)
                    cardToSpawn = card;

        if (cardToSpawn == null) // TODO : Check if string was null
            return null;

        cardToSpawn = Instantiate(cardToSpawn);
        NetworkServer.Spawn(cardToSpawn);


        if (owner)
        {
            StartCoroutine(owner.ServerReceiveCard(cardToSpawn.GetComponent<Card>().netId, owner.netId, false, false));
        }

        return cardToSpawn;
    }
}
