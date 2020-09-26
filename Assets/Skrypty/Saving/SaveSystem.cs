using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void SaveGame()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/SavedGames.puff";
        FileStream stream = new FileStream(path, FileMode.Create);

        SaveGameData data = new SaveGameData(ServerGameManager.serverGameManager.playersObjects, ServerGameManager.serverGameManager.ServerDecksManager);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static SaveGameData LoadGame()
    {
        string path = Application.persistentDataPath + "/SavedGames.puff";
        if(File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            SaveGameData data = formatter.Deserialize(stream) as SaveGameData;
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save not found in " + path);
            return null;
        }
    }
}
