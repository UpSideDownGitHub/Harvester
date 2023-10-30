using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using static UnityEditor.Progress;

[Serializable]
public struct borders
{
    [Tooltip("Top Left")]
    public Vector3Int TL;
    [Tooltip("Bottom Right")]
    public Vector3Int BR;
}

public class GridManager : NetworkBehaviour
{
    public Grid worldGrid;
    public GameObject tempObject;

    [Header("Grid Information")]
    //public List<GameObject> spawnedObjects = new();
    public Dictionary<Vector3Int, ObjectData> placedObjects = new();
    [SyncVar(OnChange = "SetData")] public Dictionary<Vector3Int, ObjectData> syncInfo = new();
    [SyncVar(OnChange = "RemoveData")] public Dictionary<Vector3Int, ObjectData> syncInfo2 = new();

    [Header("Data")]
    public PlaceableObjectsData placeables;

    [Header("NavMesh")]
    public NavMeshManager navMeshmanager;

    [Header("Save Data")]
    public PickedData pickedData;

    public override void OnStartClient()
    {
        base.OnStartClient();
        print("Owner: " + base.IsOwner);
        if (!base.IsOwner)
            GetComponent<GridManager>().enabled = false;

        // go through the list of items and spawn them all
        var saveData = SaveManager.instance.LoadMapSaveData();
        var mapData = saveData.maps[pickedData.mapID];

        for (int i = 0; i < mapData.worldPositions.Count; i++)
        {
            Vector3Int position = new Vector3Int(mapData.worldPositions[i].x,
                mapData.worldPositions[i].y,
                mapData.worldPositions[i].z);
            placedObjectGrid(mapData.itemIDs[i], position);
        }
    }

    public void SaveMapData()
    {
        // Save the current map data to the file
        var saveData = SaveManager.instance.LoadMapSaveData();
        var mapData = saveData.maps[pickedData.mapID];
        mapData.worldPositions.Clear();
        mapData.itemIDs.Clear();
        List<Vector3Int> placed = new();
        foreach (KeyValuePair<Vector3Int, ObjectData> item in placedObjects)
        {
            if (placed.Contains(item.Key))
                continue;
            Vec3 temp = new();
            temp.x = item.Value.gridSpaces[0].x;
            temp.y = item.Value.gridSpaces[0].y;
            temp.z = item.Value.gridSpaces[0].z;

            mapData.worldPositions.Add(temp);
            mapData.itemIDs.Add(item.Value.ID);
            
            for (int i = 0; i < item.Value.gridSpaces.Count; i++)
            {
                placed.Add(item.Value.gridSpaces[i]);
            }

        }
        print("Saving Data: " + mapData.worldPositions.Count + " Peices of data Saved");
        saveData.maps[pickedData.mapID] = mapData;
        SaveManager.instance.SaveMapData(saveData);
    }
    
    // BORDER INFO FOR GRID MANAGER JUST INCASE IT IS LOST
    //GenericPropertyJSON: { "name":"areaBorders","type":-1,"arraySize":9,"arrayType":"borders","children":[{ "name":"Array","type":-1,"arraySize":9,"arrayType":"borders","children":[{ "name":"size","type":12,"val":9},{ "name":"data","type":-1,"children":[{ "name":"TL","type":21,"val":"Vector3(-14,14,0)"},{ "name":"BR","type":21,"val":"Vector3(14,-14,0)"}]},{ "name":"data","type":-1,"children":[{ "name":"TL","type":21,"val":"Vector3(19,14,0)"},{ "name":"BR","type":21,"val":"Vector3(47,-14,0)"}]},{ "name":"data","type":-1,"children":[{ "name":"TL","type":21,"val":"Vector3(-14,-19,0)"},{ "name":"BR","type":21,"val":"Vector3(14,-47,0)"}]},{ "name":"data","type":-1,"children":[{ "name":"TL","type":21,"val":"Vector3(-47,14,0)"},{ "name":"BR","type":21,"val":"Vector3(-19,-14,0)"}]},{ "name":"data","type":-1,"children":[{ "name":"TL","type":21,"val":"Vector3(-14,46,0)"},{ "name":"BR","type":21,"val":"Vector3(14,19,0)"}]},{ "name":"data","type":-1,"children":[{ "name":"TL","type":21,"val":"Vector3(19,46,0)"},{ "name":"BR","type":21,"val":"Vector3(47,19,0)"}]},{ "name":"data","type":-1,"children":[{ "name":"TL","type":21,"val":"Vector3(19,-19,0)"},{ "name":"BR","type":21,"val":"Vector3(47,-47,0)"}]},{ "name":"data","type":-1,"children":[{ "name":"TL","type":21,"val":"Vector3(-47,-19,0)"},{ "name":"BR","type":21,"val":"Vector3(-19,-47,0)"}]},{ "name":"data","type":-1,"children":[{ "name":"TL","type":21,"val":"Vector3(-47,46,0)"},{ "name":"BR","type":21,"val":"Vector3(-19,19,0)"}]}]}]}
    public borders[] areaBorders;

    public GameObject ObjectClicked(Vector3 clickPos)
    {
        var worldPosision = Camera.main.ScreenToWorldPoint(clickPos);
        var gridPosition = worldGrid.WorldToCell(worldPosision);

        List<Vector3Int> clickPositions = new List<Vector3Int>() { gridPosition };
        if (isAboveAnotherObject(clickPositions))
        {
            return placedObjects[gridPosition].spawnedObject;
        }
        return null;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveObject(Vector3 position)
    {
        navMeshmanager.UpdateNavMesh();

        var gridPosition = worldGrid.WorldToCell(position);
        ObjectData data;
        if (placedObjects.ContainsKey(gridPosition))
            data = placedObjects[gridPosition];
        else
            return;
        for (int i = 0; i < data.gridSpaces.Count; i++)
        {
            if (placedObjects.ContainsKey(data.gridSpaces[i]))
                placedObjects.Remove(data.gridSpaces[i]);
        }
        syncInfo2 = placedObjects;
    }
    public void RemoveData(Dictionary<Vector3Int, ObjectData> oldValue, Dictionary<Vector3Int, ObjectData> newValue, bool asServer)
    {
        if (asServer)
            return;

        placedObjects = newValue;
    }

    public bool placedObjectGrid(int ID, Vector3Int gridPosition)
    {
        print(gridPosition);
        // check if the clicked posision is within the bounds of the current area
        if (!isAboveLand(gridPosition, placeables.placeables[ID].size))
        {
            print("Error: Can Only Place Objects On Islands");
            return false;
        }

        var gridPositions = getObjectPositions(gridPosition, placeables.placeables[ID].size);
        if (isAboveAnotherObject(gridPositions))
        {
            print("Error: Cannot Place Object Over Another Object");
            return false;
        }

        Spawn(placeables.placeables[ID].prefab, worldGrid.CellToWorld(gridPosition), Quaternion.identity, gridPositions, ID, this);

        return true;
    }
    public bool placeObjectWorld(int ID, Vector3 spawnPosition)
    {
        // get the position of the mouse
        var gridPosition = worldGrid.WorldToCell(spawnPosition);

        // check if the clicked posision is within the bounds of the current area
        if (!isAboveLand(gridPosition, placeables.placeables[ID].size))
        {
            print("Error: Can Only Place Objects On Islands");
            return false;
        }

        var gridPositions = getObjectPositions(gridPosition, placeables.placeables[ID].size);
        if (isAboveAnotherObject(gridPositions))
        {
            print("Error: Cannot Place Object Over Another Object");
            return false;
        }

        Spawn(placeables.placeables[ID].prefab, worldGrid.CellToWorld(gridPosition), Quaternion.identity, gridPositions, ID, this);

        return true;
    }

    public bool placeObject(int ID, Vector3 clickPosition)
    {
        // get the position of the mouse
        var worldPosision = Camera.main.ScreenToWorldPoint(clickPosition);
        var gridPosition = worldGrid.WorldToCell(worldPosision);

        // check if the clicked posision is within the bounds of the current area
        if (!isAboveLand(gridPosition, placeables.placeables[ID].size))
        {
            print("Error: Can Only Place Objects On Islands");
            return false;
        }

        var gridPositions = getObjectPositions(gridPosition, placeables.placeables[ID].size);
        if (isAboveAnotherObject(gridPositions))
        {
            print("Error: Cannot Place Object Over Another Object");
            return false;
        }

        Spawn(placeables.placeables[ID].prefab, worldGrid.CellToWorld(gridPosition), Quaternion.identity, gridPositions, ID, this);

        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void Spawn(GameObject objectToSpawn, Vector3 pos, Quaternion rot, List<Vector3Int> gridPositions, int ID, GridManager script)
    {
        var spawnedObject = Instantiate(objectToSpawn, pos, rot);
        ServerManager.Spawn(spawnedObject);


        SetSpawnedObjects(spawnedObject, gridPositions, ID, script);
    }
    [ServerRpc(RequireOwnership = false)]
    public void SetSpawnedObjects(GameObject spawnedObject, List<Vector3Int> gridPositions, int ID, GridManager script)
    {
        //navMeshmanager.UpdateNavMesh();

        Dictionary<Vector3Int, ObjectData> preSync = new();
        foreach (var pos in gridPositions)
        {
            var data = new ObjectData(gridPositions, ID, spawnedObject);
            placedObjects[pos] = data;
            preSync.Add(pos, data);
        }
        syncInfo = preSync;
    }
    public void SetData(Dictionary<Vector3Int, ObjectData> oldValue, Dictionary<Vector3Int, ObjectData> newValue, bool asServer)
    {
        if (asServer)
            return;

        foreach (KeyValuePair<Vector3Int, ObjectData> item in newValue)
        {
            placedObjects[item.Key] = item.Value;
        }
    }

    public List<Vector3Int> getObjectPositions(Vector3Int gridPos, Vector2Int size)
    {
        List<Vector3Int> tempList = new();
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                Vector3Int pos = new Vector3Int(gridPos.x + i, gridPos.y + j, gridPos.z);
                tempList.Add(pos);
            }
        }
        return tempList;
    }

    public bool isAboveAnotherObject(List<Vector3Int> positions)
    {
        foreach (var pos in positions)
        {
            if (placedObjects.ContainsKey(pos))
                return true;
        }
        return false;
    }

    public bool isAboveLand(Vector3Int gridPos, Vector2Int size)
    {
        
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                Vector3Int pos = new Vector3Int(gridPos.x + i, gridPos.y + j, gridPos.z);
                bool canSpawn = false;
                for (int k = 0; k < areaBorders.Length; k++)
                {
                    if (inRange(pos, areaBorders[k].TL.x, areaBorders[k].BR.x, areaBorders[k].BR.y, areaBorders[k].TL.y))
                        canSpawn = true;
                }
                if (!canSpawn)
                    return false;
            }
        }
        return true;
    }

    bool inRange(Vector3Int pos, int xMin = -100, int xMax = 100, int yMin = -100, int yMax = 100) =>
        ((pos.x - xMin) * (pos.x - xMax) <= 0) && ((pos.y - yMin) * (pos.y - yMax) <= 0);
}

[Serializable]
public class ObjectData
{
    public List<Vector3Int> gridSpaces;
    public int ID;
    public GameObject spawnedObject;

    public ObjectData() { }
    public ObjectData(List<Vector3Int> gridSpaces, int ID, GameObject spawnedObject)
    {
        this.gridSpaces = gridSpaces;
        this.ID = ID;
        this.spawnedObject = spawnedObject;
    }
}