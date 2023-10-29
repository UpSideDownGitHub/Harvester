using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    // public variables
    public static SaveManager instance;

    // called when the object is being loaded
    void Awake()
    {
        // Make Singleton
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // save the current data to the file
    public void SaveMapData(MapSaveData data)
    {
        string temp = JsonConvert.SerializeObject(data);
        File.WriteAllText(Application.persistentDataPath + "/mapSaveData.json", temp);
    }
    public void SavePlayerData(PlayerSaveData data)
    {
        string temp = JsonConvert.SerializeObject(data);
        File.WriteAllText(Application.persistentDataPath + "/playerSaveData.json", temp);
    }

    public MapSaveData LoadMapSaveData()
    {
        if (FileExists(Application.persistentDataPath + "/mapSaveData.json"))
        {
            string temp = File.ReadAllText(Application.persistentDataPath + "/mapSaveData.json");
            var data = JsonConvert.DeserializeObject<MapSaveData>(temp);
            return data;
        }
        SaveMapData(new MapSaveData());
        return LoadMapSaveData();
    }
    public PlayerSaveData LoadPlayerSaveData()
    {
        if (FileExists(Application.persistentDataPath + "/playerSaveData.json"))
        {
            string temp = File.ReadAllText(Application.persistentDataPath + "/playerSaveData.json");
            var data = JsonConvert.DeserializeObject<PlayerSaveData>(temp);
            return data;
        }
        SavePlayerData(new PlayerSaveData());
        return LoadPlayerSaveData();
    }

    public bool FileExists(string file) { return File.Exists(file); }
}

// save data classes
[Serializable]
public class MapSaveData
{
    public List<MapData> maps = new();
}
[Serializable]
public class MapData
{
    public MapData(string name) { mapName = name; }

    public string mapName;
    public Dictionary<Vector3Int, ObjectData> world = new();
}

[Serializable]
public class PlayerSaveData
{
    public List<PlayerData> players = new();
}

[Serializable]
public class PlayerData
{
    public PlayerData(string name) { playerName = name; }

    public string playerName;
    public Dictionary<Item, int> inventory = new();
    public Dictionary<Item, bool> hotbar = new();
}
