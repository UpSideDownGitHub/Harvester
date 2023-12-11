using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public struct borders
{
    [Tooltip("Top Left")]
    public Vector3Int TL;
    [Tooltip("Bottom Right")]
    public Vector3Int BR;
}

public class GridManager : MonoBehaviour
{
    public Grid worldGrid;
    public GameObject tempObject;

    [Header("Grid Information")]
    public Dictionary<Vector3Int, ObjectData> placedObjects = new();

    [Header("Data")]
    public PlaceableObjectsData placeables;
    public ItemSpawner spawner;

    [Header("NavMesh")]
    public NavMeshManager navMeshmanager;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip placeItem;

    public void Start()
    {
        PhotonView photonView = PhotonView.Get(this);

        if (!photonView.IsMine)
            GetComponent<GridManager>().enabled = false;

        // go through the list of items and spawn them all
        var pickedData = SaveManager.instance.LoadGeneralSaveData();
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
        var pickedData = SaveManager.instance.LoadGeneralSaveData();
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
        mapData.spawns = spawner.currentSpawns;
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
            return GameObject.Find(placedObjects[gridPosition].objectName);
        }
        return null;
    }

    [PunRPC]
    public void RemoveObject(Vector3 position)
    {
        //navMeshmanager.UpdateNavMesh();
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
    }

    [PunRPC]
    public void SetSpawnedObjects(string objectName, object[] recivedPositions, int ID)
    {
        List<Vector3Int> gridPositions = new List<Vector3Int>();
        for (int i = 0; i < recivedPositions.Length; i++)
        {
            Vector3Int tempVec = new Vector3Int((int)((Vector3)recivedPositions[i]).x, (int)((Vector3)recivedPositions[i]).y, (int)((Vector3)recivedPositions[i]).z);
            gridPositions.Add(tempVec);
        }

        //navMeshmanager.UpdateNavMesh();
        foreach (var pos in gridPositions)
        {
            var data = new ObjectData(gridPositions, ID, objectName);
            placedObjects.Add(pos, data);
        }
    }


    public void Spawn(GameObject objectToSpawn, Vector3 pos, Quaternion rot, List<Vector3Int> gridPositions, int ID)
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            audioSource.PlayOneShot(placeItem);

            // need to convert gridPosiions to object[]
            object[] dataToSend = new object[gridPositions.Count];
            for (int i = 0; i < gridPositions.Count; i++)
            {
                dataToSend[i] = new Vector3(gridPositions[i].x, gridPositions[i].y, gridPositions[i].z);
            }

            var spawnedObject = PhotonNetwork.Instantiate("Placeables/" + objectToSpawn.name, pos, rot, 0);

            // Chnage name for all instances of this object
            PhotonView objectView = PhotonView.Get(spawnedObject);
            objectView.RPC("SetName", RpcTarget.All, spawnedObject.GetInstanceID().ToString());

            // set on grid for all instances of object
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("SetSpawnedObjects", RpcTarget.All, spawnedObject.name, dataToSend, ID);
        }
    }

    public bool placedObjectGrid(int ID, Vector3Int gridPosition)
    {
        //print(gridPosition);
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

        Spawn(placeables.placeables[ID].prefab, worldGrid.CellToWorld(gridPosition), Quaternion.identity, gridPositions, ID);

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

        Spawn(placeables.placeables[ID].prefab, worldGrid.CellToWorld(gridPosition), Quaternion.identity, gridPositions, ID);

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

        Spawn(placeables.placeables[ID].prefab, worldGrid.CellToWorld(gridPosition), Quaternion.identity, gridPositions, ID);

        return true;
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
    public string objectName;

    public ObjectData() { }
    public ObjectData(List<Vector3Int> gridSpaces, int ID, string objectName)
    {
        this.gridSpaces = gridSpaces;
        this.ID = ID;
        this.objectName = objectName;
    }
}