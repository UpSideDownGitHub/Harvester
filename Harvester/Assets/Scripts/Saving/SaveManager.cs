
//#define TEST

using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.AccessControl;
using UnityEngine;


public class SaveManager : MonoBehaviour
{
    // public variables
    public static SaveManager instance;

/// <summary>
/// Initializes the singleton instance of the script and ensures that only one instance exists.
/// Also performs additional setup such as creating test save data and logging the persistent data path.
/// </summary>
/// <remarks>
/// This method sets up the singleton instance and performs additional setup actions, such as creating test save data in debug mode
/// and logging the application's persistent data path.
/// </remarks>
    void Awake()
    {
        // Make Singleton
        if (instance == null) instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(this);

#if TEST
        MapSaveData mapData = LoadMapSaveData();
        mapData.maps.Clear();
        mapData.maps.Add(new MapData("MAP"));
        SaveMapData(mapData);
        PlayerSaveData playerData = LoadPlayerSaveData();
        playerData.players.Clear();
        playerData.players.Add(new PlayerData("PLAYER"));
        SavePlayerData(playerData);
#endif

        Debug.Log(Application.persistentDataPath);
    }

/// <summary>
/// Saves the provided general data to the general save data file.
/// </summary>
/// <param name="data">The general data to be saved.</param>
    public void SaveGeneralData(GeneralSaveData data)
    {
        string temp = JsonConvert.SerializeObject(data);
        File.WriteAllText(Application.persistentDataPath + "/generalSaveData.json", temp);
    }
/// <summary>
/// Saves the provided map data to the map save data file.
/// </summary>
/// <param name="data">The map data to be saved.</param>
    public void SaveMapData(MapSaveData data)
    {
        string temp = JsonConvert.SerializeObject(data);
        File.WriteAllText(Application.persistentDataPath + "/mapSaveData.json", temp);
    }
/// <summary>
/// Saves the provided player data to the player save data file.
/// </summary>
/// <param name="data">The player data to be saved.</param>
    public void SavePlayerData(PlayerSaveData data)
    {
        string temp = JsonConvert.SerializeObject(data);
        File.WriteAllText(Application.persistentDataPath + "/playerSaveData.json", temp);
    }

/// <summary>
/// Loads the general save data from the general save data file.
/// If the file doesn't exist, creates a new general save data file.
/// </summary>
/// <returns>The loaded or newly created general save data.</returns>
    public GeneralSaveData LoadGeneralSaveData()
    {
        if (!FileExists(Application.persistentDataPath + "/generalSaveData.json"))
            SaveGeneralData(new GeneralSaveData());
        string temp = File.ReadAllText(Application.persistentDataPath + "/generalSaveData.json");
        return JsonConvert.DeserializeObject<GeneralSaveData>(temp);
    }
/// <summary>
/// Loads the map save data from the map save data file.
/// If the file doesn't exist, creates a new map save data file.
/// </summary>
/// <returns>The loaded or newly created map save data.</returns>
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
/// <summary>
/// Loads the player save data from the player save data file.
/// If the file doesn't exist, creates a new player save data file.
/// </summary>
/// <returns>The loaded or newly created player save data.</returns>
    public PlayerSaveData LoadPlayerSaveData()
    {
        if (FileExists(Application.persistentDataPath + "/playerSaveData.json"))
        {
            string temp = File.ReadAllText(Application.persistentDataPath + "/playerSaveData.json");
            var data = JsonConvert.DeserializeObject<PlayerSaveData>(temp);
            return data;
        }
#if TEST
        var playerSave = new PlayerSaveData();
        playerSave.players.Add(new PlayerData("Test Player"));
        // Add the base tools to the players inventory
        // ID: 5,6,7 (the base tools IDs) 
        playerSave.players[0].inventory.Add(5, 1);
        playerSave.players[0].inventory.Add(6, 1);
        playerSave.players[0].inventory.Add(7, 1);
        playerSave.players[0].hotbar.Add(5, true);
        playerSave.players[0].hotbar.Add(6, true);
        playerSave.players[0].hotbar.Add(7, true);
        SavePlayerData(playerSave);
        return LoadPlayerSaveData();
#else
        SavePlayerData(new PlayerSaveData());
        return LoadPlayerSaveData();
#endif

    }

/// <summary>
/// Checks if the specified file exists.
/// </summary>
/// <param name="file">The path to the file.</param>
/// <returns>True if the file exists; false otherwise.</returns>
    public bool FileExists(string file) { return File.Exists(file); }
}

// save data classes
[Serializable]
public class GeneralSaveData
{
    public int mapID;
    public int playerID;
}
[Serializable]
public class MapSaveData
{
    public List<MapData> maps = new();
}
[Serializable]
public class MapData
{
    public MapData(string name) 
    { 
        mapName = name;
        section1Unlocked = false;
        section2Unlocked = false;
        section3Unlocked = false;
        section4Unlocked = false;
    }

    public int[] spawns = new int[5];
    public string mapName;
    public bool section1Unlocked;
    public bool section2Unlocked;
    public bool section3Unlocked;
    public bool section4Unlocked;
    // placeable position, and placeable ID
    public List<Vec3> worldPositions = new();
    public List<int> itemIDs = new();
}
[Serializable]
public class Vec3
{
    public int x;
    public int y;
    public int z;
}

[Serializable]
public class PlayerSaveData
{
    public List<PlayerData> players = new();
}

[Serializable]
public class PlayerData
{
    public PlayerData(string name) { playerName = name; hearts = 3; }

    public string playerName;
    public int hearts;
    // item ID and then Count and if it is in hotbar
    public Dictionary<int, int> inventory = new();
    public Dictionary<int, bool> hotbar = new();
}
