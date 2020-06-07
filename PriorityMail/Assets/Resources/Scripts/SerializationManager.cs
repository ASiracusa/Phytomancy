using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class SerializationManager
{
    public static bool SaveLevel (string worldName, string levelName, object levelData)
    {
        BinaryFormatter formatter = GetBinaryFormatter();

        if (!Directory.Exists(Application.persistentDataPath + "/worlds"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/worlds");
        }

        if (!Directory.Exists(Application.persistentDataPath + "/worlds/" + worldName))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/worlds/" + worldName);
        }

        string path = Application.persistentDataPath + "/worlds/" + worldName + "/" + levelName + ".lvl";

        FileStream file = File.Create(path);

        formatter.Serialize(file, levelData);

        file.Close();

        return true;
    }

    public static object LoadLevel (string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        BinaryFormatter formatter = GetBinaryFormatter();

        FileStream file = File.Open(path, FileMode.Open);

        try
        {
            object save = formatter.Deserialize(file);
            file.Close();
            return save;
        }
        catch
        {
            Debug.LogErrorFormat("Failed to load file at {0}", path);
            file.Close();
            return null;
        }
    }

    public static BinaryFormatter GetBinaryFormatter()
    {
        BinaryFormatter formatter = new BinaryFormatter();

        return formatter;
    }
}
