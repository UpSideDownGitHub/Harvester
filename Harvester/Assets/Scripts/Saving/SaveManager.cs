﻿
#define TEST

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

    // called when the object is being loaded
    void Awake()
    {
        // Make Singleton
        if (instance == null) instance = this;
        else Destroy(gameObject);

        /*
        MapSaveData mapData = LoadMapSaveData();
        mapData.maps.Clear();
        mapData.maps.Add(new MapData("TEST MAP PLEEASE DELETE THESE LINES"));
        SaveMapData(mapData);
        PlayerSaveData playerData = LoadPlayerSaveData();
        playerData.players.Clear();
        playerData.players.Add(new PlayerData("TEST PLAYER NAME"));
        SavePlayerData(playerData);
        */

        Debug.Log(Application.persistentDataPath);
    }

    // save the current data to the file
    public void SaveMapData(MapSaveData data)
    {
        string temp = JsonConvert.SerializeObject(data);
        File.WriteAllText(Application.persistentDataPath + "/mapSaveData.json", temp);
        print("Saved Data");
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
#if TEST
        var mapSave = new MapSaveData();
        mapSave.maps.Add(new MapData("Test Map"));
        SaveMapData(mapSave);
        return LoadMapSaveData();
#else
        SaveMapData(new MapSaveData());
        return LoadMapSaveData();
#endif
    }
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
    public MapData(string name) 
    { 
        mapName = name;
        section1Unlocked = false;
        section2Unlocked = false;
        section3Unlocked = false;
        section4Unlocked = false;
    }

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
    public PlayerData(string name) { playerName = name; }

    public string playerName;
    // item ID and then Count and if it is in hotbar
    public Dictionary<int, int> inventory = new();
    public Dictionary<int, bool> hotbar = new();
}
