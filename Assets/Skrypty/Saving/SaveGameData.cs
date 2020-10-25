using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class SaveGameData
{
    public List<PlayerSaveData> playersData;

    public List<string> discardedCards;
    public TurnPhase turnPhase;

    public SaveGameData()
    {
        // Saving Players

        if (!ServerGameManager.serverGameManager)
            return;

        turnPhase = ServerGameManager.serverGameManager.turnPhase;

        playersData = new List<PlayerSaveData>();

        foreach (var player in ServerGameManager.serverGameManager.playersObjects)
        {
            //Debug.Log("Save Player: " + player);
            playersData.Add(player.GetComponent<PlayerInGame>().GetPlayerData());
        }

        // Saving Cards that are in decks

        discardedCards = new List<string>();

        ServerDecksManager serverDecksManager = ServerGameManager.ServerDecksManager;

        for (int i = 0; i < serverDecksManager.DiscardedDoorsDeck.childCount; i++)
            discardedCards.Add(serverDecksManager.DiscardedDoorsDeck.GetChild(i).GetComponent<Card>().GetCardData());

        for (int i = 0; i < serverDecksManager.DiscardedTreasuresDeck.childCount; i++)
            discardedCards.Add(serverDecksManager.DiscardedTreasuresDeck.GetChild(i).GetComponent<Card>().GetCardData());

        for (int i = 0; i < serverDecksManager.DiscardedSpellsDeck.childCount; i++)
            discardedCards.Add(serverDecksManager.DiscardedSpellsDeck.GetChild(i).GetComponent<Card>().GetCardData());

        for (int i = 0; i < serverDecksManager.DiscardedHelpHandsDeck.childCount; i++)
            discardedCards.Add(serverDecksManager.DiscardedHelpHandsDeck.GetChild(i).GetComponent<Card>().GetCardData());
    }
}
