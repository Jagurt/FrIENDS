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

    public static SaveGameData LoadGame()
    {
        throw new System.NotImplementedException();

        string path = Application.persistentDataPath + "/TestSave.json";
        if (File.Exists(path))
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
