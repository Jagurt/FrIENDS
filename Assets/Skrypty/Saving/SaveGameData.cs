using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class SaveGameData
{
    public List<PlayerSaveData> playersData;

    public List<string> cardsInDoorsDeck;
    public List<string> cardsInTreasuresDeck;
    public List<string> cardsInSpellsDeck;
    public List<string> cardsInHelpHandsDeck;
    public List<string> cardsInDiscardedDoorsDeck;
    public List<string> cardsInDiscardedTreasuresDeck;
    public List<string> cardsInDiscardedSpellsDeck;
    public List<string> cardsInDiscardedHelpHandsDeck;

    public SaveGameData()
    {
        // Saving Players

        playersData = new List<PlayerSaveData>();

        foreach (var player in ServerGameManager.serverGameManager.playersObjects)
        {
            Debug.Log("Save Player: " + player);
            playersData.Add(player.GetComponent<PlayerInGame>().GetPlayerData());
        }

        // Saving Cards that are in decks

        cardsInDoorsDeck = new List<string>();
        cardsInTreasuresDeck = new List<string>();
        cardsInSpellsDeck = new List<string>();
        cardsInHelpHandsDeck = new List<string>();
        cardsInDiscardedDoorsDeck = new List<string>();
        cardsInDiscardedTreasuresDeck = new List<string>();
        cardsInDiscardedSpellsDeck = new List<string>();
        cardsInDiscardedHelpHandsDeck = new List<string>();

        ServerDecksManager serverDecksManager = ServerGameManager.serverGameManager.ServerDecksManager;

        for (int i = 0; i < serverDecksManager.DoorsDeck.childCount; i++)
            cardsInDoorsDeck.Add(serverDecksManager.DoorsDeck.GetChild(i).GetComponent<Card>().GetCardData());

        for (int i = 0; i < serverDecksManager.TreasuresDeck.childCount; i++)
            cardsInDoorsDeck.Add(serverDecksManager.TreasuresDeck.GetChild(i).GetComponent<Card>().GetCardData());

        for (int i = 0; i < serverDecksManager.SpellsDeck.childCount; i++)
            cardsInDoorsDeck.Add(serverDecksManager.SpellsDeck.GetChild(i).GetComponent<Card>().GetCardData());

        for (int i = 0; i < serverDecksManager.HelpHandsDeck.childCount; i++)
            cardsInDoorsDeck.Add(serverDecksManager.HelpHandsDeck.GetChild(i).GetComponent<Card>().GetCardData());

        for (int i = 0; i < serverDecksManager.DiscardedDoorsDeck.childCount; i++)
            cardsInDoorsDeck.Add(serverDecksManager.DiscardedDoorsDeck.GetChild(i).GetComponent<Card>().GetCardData());

        for (int i = 0; i < serverDecksManager.DiscardedTreasuresDeck.childCount; i++)
            cardsInDoorsDeck.Add(serverDecksManager.DiscardedTreasuresDeck.GetChild(i).GetComponent<Card>().GetCardData());

        for (int i = 0; i < serverDecksManager.DiscardedSpellsDeck.childCount; i++)
            cardsInDoorsDeck.Add(serverDecksManager.DiscardedSpellsDeck.GetChild(i).GetComponent<Card>().GetCardData());

        for (int i = 0; i < serverDecksManager.DiscardedHelpHandsDeck.childCount; i++)
            cardsInDoorsDeck.Add(serverDecksManager.DiscardedHelpHandsDeck.GetChild(i).GetComponent<Card>().GetCardData());
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
