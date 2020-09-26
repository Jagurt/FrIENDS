using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SaveGameData
{
    //TODO : zamienić tablice na listy, bo da się i jest łatwiej (?)

    public List<string> cardsInDoorsDeck = new List<string>();
    public List<string> cardsInTreasuresDeck = new List<string>();
    public List<string> cardsInSpellsDeck = new List<string>();
    public List<string> cardsInHelpHandsDeck = new List<string>();
    public List<string> cardsInDiscardedDoorsDeck = new List<string>();
    public List<string> cardsInDiscardedTreasuresDeck = new List<string>();
    public List<string> cardsInDiscardedSpellsDeck = new List<string>();
    public List<string> cardsInDiscardedHelpHandsDeck = new List<string>();

    public List<short> playerXLevel = new List<short>();
    public List<bool> isPlayerXAlive = new List<bool>();

    public List<List<string>> cardsInXHand = new List<List<string>>();

    public List<string> playerXhead = new List<string>();
    public List<string> playerXchest = new List<string>();
    public List<string> playerXhands = new List<string>();
    public List<string> playerXlegs = new List<string>();
    public List<string> playerXfeet = new List<string>();
    public List<string> playerXring = new List<string>();
    public List<string> playerXweapon1 = new List<string>();
    public List<string> playerXweapon2 = new List<string>();

    public SaveGameData( List<GameObject> playersGO, ServerDecksManager decks )
    {
        List<PlayerInGame> players = new List<PlayerInGame>();

        foreach (var playerGO in playersGO)
        {
            players.Add(playerGO.GetComponent<PlayerInGame>());
        }

        int i = 0;

        foreach (var player in players)
        {
            playerXLevel.Add(player.Level);
            isPlayerXAlive.Add(player.isPlayerAlive);

            playerXhead.Add(player.GetEqFrom(EqPart.Head));
            playerXchest.Add(player.GetEqFrom(EqPart.Chest));
            playerXhands.Add(player.GetEqFrom(EqPart.Hands));
            playerXlegs.Add(player.GetEqFrom(EqPart.Legs));
            playerXfeet.Add(player.GetEqFrom(EqPart.Feet));
            playerXring.Add(player.GetEqFrom(EqPart.Ring));
            playerXweapon1.Add(player.GetEqFrom(EqPart.Weapon1));
            playerXweapon2.Add(player.GetEqFrom(EqPart.Weapon2));

            cardsInXHand.Add(new List<string>());

            for (int j = 0; j < player.handContent.childCount; j++)
            {
                cardsInXHand[i].Add(player.handContent.GetChild(j).name);
            }

            i++;
        }

        for (int j = 0; j < decks.DoorsDeck.childCount; j++)
            cardsInDoorsDeck.Add(decks.DoorsDeck.GetChild(j).name);

        for (int j = 0; j < decks.TreasuresDeck.childCount; j++)
            cardsInTreasuresDeck.Add(decks.TreasuresDeck.GetChild(j).name);

        for (int j = 0; j < decks.SpellsDeck.childCount; j++)
            cardsInSpellsDeck.Add(decks.SpellsDeck.GetChild(j).name);

        for (int j = 0; j < decks.HelpHandsDeck.childCount; j++)
            cardsInHelpHandsDeck.Add(decks.HelpHandsDeck.GetChild(j).name);

        for (int j = 0; j < decks.DiscardedDoorsDeck.childCount; j++)
            cardsInDiscardedDoorsDeck.Add(decks.DiscardedDoorsDeck.GetChild(j).name);

        for (int j = 0; j < decks.DiscardedTreasuresDeck.childCount; j++)
            cardsInDiscardedTreasuresDeck.Add(decks.DiscardedTreasuresDeck.GetChild(j).name);

        for (int j = 0; j < decks.DiscardedSpellsDeck.childCount; j++)
            cardsInDiscardedSpellsDeck.Add(decks.DiscardedSpellsDeck.GetChild(j).name);

        for (int j = 0; j < decks.DiscardedHelpHandsDeck.childCount; j++)
            cardsInDiscardedHelpHandsDeck.Add(decks.DiscardedHelpHandsDeck.GetChild(j).name);
    }
}


/*
 * using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SaveGameData
{
    //TODO : zamienić tablice na listy, bo da się i jest łatwiej

    public string[] cardsInDoorsDeck;
    public string[] cardsInTreasuresDeck;
    public string[] cardsInSpellsDeck;
    public string[] cardsInHelpHandsDeck;
    public string[] cardsInDiscardedDoorsDeck;
    public string[] cardsInDiscardedTreasuresDeck;
    public string[] cardsInDiscardedSpellsDeck;
    public string[] cardsInDiscardedHelpHandsDeck;

    public int[] playerXLevel;
    public bool[] isPlayerXAlive;

    public string[,] cardsInXHand;

    public string[] playerXhead;
    public string[] playerXchest;
    public string[] playerXhands;
    public string[] playerXlegs;
    public string[] playerXfeet;
    public string[] playerXring;
    public string[] playerXweapon1;
    public string[] playerXweapon2;

    public SaveGameData( GameObject[] playersGO, ServerDecksManager decks )
    {
        PlayerInGame[] players = new PlayerInGame[playersGO.Length];

        for (int i = 0; i < playersGO.Length; i++)
        {
            players[i] = playersGO[i].GetComponent<PlayerInGame>();
        }

        playerXLevel = new int[players.Length];
        isPlayerXAlive = new bool[players.Length];

        playerXhead = new string[players.Length];
        playerXchest = new string[players.Length];
        playerXhands = new string[players.Length];
        playerXlegs = new string[players.Length];
        playerXfeet = new string[players.Length];
        playerXring = new string[players.Length];
        playerXweapon1 = new string[players.Length];
        playerXweapon2 = new string[players.Length];

        int maxCardsInHand = 0;

        for (int i = 0; i < players.Length; i++)
        {
            playerXLevel[i] = players[i].Level;
            isPlayerXAlive[i] = players[i].isPlayerAlive;
            playerXhead[i] = players[i].GetEqFrom(EqPart.Head);
            playerXchest[i] = players[i].GetEqFrom(EqPart.Chest);
            playerXhands[i] = players[i].GetEqFrom(EqPart.Hands);
            playerXlegs[i] = players[i].GetEqFrom(EqPart.Legs);
            playerXfeet[i] = players[i].GetEqFrom(EqPart.Feet);
            playerXring[i] = players[i].GetEqFrom(EqPart.Ring);
            playerXweapon1[i] = players[i].GetEqFrom(EqPart.Weapon1);
            playerXweapon2[i] = players[i].GetEqFrom(EqPart.Weapon2);

            if (maxCardsInHand < players[i].handPanelTransform.childCount)
                maxCardsInHand = players[i].handPanelTransform.childCount;
        }

        cardsInXHand = new string[players.Length, maxCardsInHand];

        for (int i = 0; i < players.Length; i++)
        {
            for (int j = 0; j < maxCardsInHand; j++)
            {
                if (j < players[i].handPanelTransform.childCount)
                    cardsInXHand[i, j] = players[i].handPanelTransform.GetChild(j).name;
                else
                    cardsInXHand[i, j] = null;
            }
        }

        cardsInDoorsDeck = new string[decks.DoorsDeck.childCount];
        cardsInTreasuresDeck = new string[decks.TreasuresDeck.childCount];
        cardsInSpellsDeck = new string[decks.SpellsDeck.childCount];
        cardsInHelpHandsDeck = new string[decks.HelpHandsDeck.childCount];
        cardsInDiscardedDoorsDeck = new string[decks.DiscardedDoorsDeck.childCount];
        cardsInDiscardedTreasuresDeck = new string[decks.DiscardedTreasuresDeck.childCount];
        cardsInDiscardedSpellsDeck = new string[decks.DiscardedSpellsDeck.childCount];
        cardsInDiscardedHelpHandsDeck = new string[decks.DiscardedHelpHandsDeck.childCount];

        for (int i = 0; i < cardsInDoorsDeck.Length; i++)
            cardsInDoorsDeck[i] = decks.DoorsDeck.GetChild(i).name;

        for (int i = 0; i < cardsInTreasuresDeck.Length; i++)
            cardsInTreasuresDeck[i] = decks.TreasuresDeck.GetChild(i).name;

        for (int i = 0; i < cardsInSpellsDeck.Length; i++)
            cardsInSpellsDeck[i] = decks.SpellsDeck.GetChild(i).name;

        for (int i = 0; i < cardsInHelpHandsDeck.Length; i++)
            cardsInHelpHandsDeck[i] = decks.HelpHandsDeck.GetChild(i).name;

        for (int i = 0; i < cardsInDiscardedDoorsDeck.Length; i++)
            cardsInDiscardedDoorsDeck[i] = decks.DiscardedDoorsDeck.GetChild(i).name;

        for (int i = 0; i < cardsInDiscardedTreasuresDeck.Length; i++)
            cardsInDiscardedTreasuresDeck[i] = decks.DiscardedTreasuresDeck.GetChild(i).name;

        for (int i = 0; i < cardsInDiscardedSpellsDeck.Length; i++)
            cardsInDiscardedSpellsDeck[i] = decks.DiscardedSpellsDeck.GetChild(i).name;

        for (int i = 0; i < cardsInDiscardedHelpHandsDeck.Length; i++)
            cardsInDiscardedHelpHandsDeck[i] = decks.DiscardedHelpHandsDeck.GetChild(i).name;
    }
}
*/
