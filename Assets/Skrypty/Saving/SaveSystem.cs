using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void SaveGame()
    {
        string gameData = JsonUtility.ToJson(new SaveGameData());
        File.WriteAllText(Application.persistentDataPath + "/TestSave.json", gameData);

        Debug.Log("Saving in: " + Application.persistentDataPath);
    }

    // [Server]
    public static void LoadGame()
    {
        string path = Application.persistentDataPath + "/TestSave.json";

        SaveGameData gameData = JsonUtility.FromJson<SaveGameData>(File.ReadAllText(path));

        if (ServerGameManager.serverGameManager.playersObjects.Count != gameData.playersData.Count)
        {
            InfoPanel.Alert("Number of players do not match!");
            return;
        }

        // Loading Players

        for (int i = 0; i < gameData.playersData.Count; i++)
        {
            ServerGameManager.serverGameManager.playersObjects[i].GetComponent<PlayerInGame>().Level = gameData.playersData[i].level;
            ServerGameManager.serverGameManager.playersObjects[i].GetComponent<PlayerInGame>().hasTurn = gameData.playersData[i].hasTurn;
            ServerGameManager.serverGameManager.playersObjects[i].GetComponent<PlayerInGame>().isAlive = gameData.playersData[i].isAlive;

            for (int j = 0; j < gameData.playersData[i].cardsInHand.Count; j++)
            {
                GameObject card = ServerGameManager.serverGameManager.GetCardByName(gameData.playersData[i].cardsInHand[j]);
                ServerGameManager.serverGameManager.playersObjects[i].GetComponent<PlayerInGame>().OnLoadReceiveCard(card);
            }

            for (int j = 0; j < gameData.playersData[i].equippedItems.Count; j++)
            {
                GameObject card = ServerGameManager.serverGameManager.GetCardByName(gameData.playersData[i].equippedItems[j]);
                ServerGameManager.serverGameManager.playersObjects[i].GetComponent<PlayerInGame>().OnLoadEquip(card);
            }
        }

        // Loading Decks

        for (int i = 0; i < gameData.discardedCards.Count; i++)
        {
            GameObject card = ServerGameManager.serverGameManager.GetCardByName(gameData.discardedCards[i]);
            ServerGameManager.serverGameManager.playersObjects[0].GetComponent<PlayerInGame>().OnLoadDiscardCard(card);
        }
    }
}
