using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static SaveGameData loadedSave;
    static string path = Application.persistentDataPath + "/TestSave.json";

    public static void SaveGame()
    {
        string gameData = JsonUtility.ToJson(new SaveGameData());
        File.WriteAllText(path, gameData);

        Debug.Log("Saving in: " + path);
    }

    // [Server]
    public static void LoadObjectsToGame( SaveGameData save )
    {
        // Loading Players

        for (int i = 0; i < save.playersData.Count; i++)
        {
            ServerGameManager.serverGameManager.playersObjects[i].GetComponent<PlayerInGame>().Level = save.playersData[i].level;
            ServerGameManager.serverGameManager.playersObjects[i].GetComponent<PlayerInGame>().hasTurn = save.playersData[i].hasTurn;
            ServerGameManager.serverGameManager.playersObjects[i].GetComponent<PlayerInGame>().isAlive = save.playersData[i].isAlive;

            for (int j = 0; j < save.playersData[i].cardsInHand.Count; j++)
            {
                GameObject card = ServerGameManager.serverGameManager.GetCardByName(save.playersData[i].cardsInHand[j]);
                ServerGameManager.serverGameManager.playersObjects[i].GetComponent<PlayerInGame>().OnLoadReceiveCard(card);
            }

            for (int j = 0; j < save.playersData[i].equippedItems.Count; j++)
            {
                GameObject card = ServerGameManager.serverGameManager.GetCardByName(save.playersData[i].equippedItems[j]);
                ServerGameManager.serverGameManager.playersObjects[i].GetComponent<PlayerInGame>().OnLoadEquip(card);
            }
        }

        // Loading Decks

        for (int i = 0; i < save.discardedCards.Count; i++)
        {
            GameObject card = ServerGameManager.serverGameManager.GetCardByName(save.discardedCards[i]);
            ServerGameManager.serverGameManager.playersObjects[0].GetComponent<PlayerInGame>().OnLoadDiscardCard(card);
        }
    }

    public static SaveGameData LoadGame()
    {
        loadedSave = JsonUtility.FromJson<SaveGameData>(File.ReadAllText(path));
        return loadedSave;
    }
}
