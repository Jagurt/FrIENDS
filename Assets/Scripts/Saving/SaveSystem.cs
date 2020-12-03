using UnityEngine;
using System.IO;
using System.Collections;

public static class SaveSystem
{
    public static SaveGameData loadedSave;
    public static string savesFolderPath = Application.persistentDataPath + "/Saves/";

    public static void SaveGame(string path)
    {
        string gameData = JsonUtility.ToJson(new SaveGameData());
        File.WriteAllText(path, gameData);
        Debug.Log("Saving in: " + path);
    }

    public static void AutoSaveGame()
    {
        string path = savesFolderPath + "Autosave";
        string gameData = JsonUtility.ToJson(new SaveGameData());
        File.WriteAllText(path, gameData);
        Debug.Log("Saving in: " + path);
    }

    /// <summary>
    /// Setting players stats and objects in hierarchy based on Saved Data.
    /// </summary>
    // [Server]
    public static IEnumerator LoadGame()
    {
        CustomNetManager CustomNetworkManager = CustomNetManager.singleton;

        if (CustomNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.isServerBusy);
        CustomNetworkManager.isServerBusy = true;

        // Loading Players
        GameManager serverGameManager = GameManager.singleton;

        serverGameManager.turnPhase = loadedSave.turnPhase;
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < loadedSave.playersData.Count; i++)
        {
            GameObject playerGO = serverGameManager.playersObjects[i];
            PlayerInGame playerScript = playerGO.GetComponent<PlayerInGame>();

            playerScript.Level = loadedSave.playersData[i].level;
            yield return new WaitForEndOfFrame();

            playerScript.hasTurn = loadedSave.playersData[i].hasTurn;
            yield return new WaitForEndOfFrame();

            if (playerScript.hasTurn)
            {
                serverGameManager.activePlayerIndex = i;
                yield return new WaitForEndOfFrame();
            }

            playerScript.isAlive = loadedSave.playersData[i].isAlive;
            yield return new WaitForEndOfFrame();

            for (int j = 0; j < loadedSave.playersData[i].cardsInHand.Count; j++)
            {
                GameObject card = GameManager.GetCardByName(loadedSave.playersData[i].cardsInHand[j]);
                playerScript.ServerOnLoadReceiveCard(card);
                yield return new WaitForEndOfFrame();
            }

            for (int j = 0; j < loadedSave.playersData[i].equippedItems.Count; j++)
            {
                GameObject card = GameManager.GetCardByName(loadedSave.playersData[i].equippedItems[j]);
                playerScript.ServerOnLoadEquip(card);
                yield return new WaitForEndOfFrame();
            }
        }

        // Loading Decks
        for (int i = 0; i < loadedSave.discardedCards.Count; i++)
        {
            GameObject card = GameManager.GetCardByName(loadedSave.discardedCards[i]);
            PlayerInGame.OnLoadDiscardCard(card);
            yield return new WaitForEndOfFrame();
        }

        CustomNetworkManager.isServerBusy = false;

        serverGameManager.StartCoroutine(serverGameManager.ServerTurnOwnerReadiness());
    }

    public static SaveGameData LoadSaveFile(string saveFilePath)
    {
        loadedSave = JsonUtility.FromJson<SaveGameData>(File.ReadAllText(saveFilePath));
        return loadedSave;
    }
}
