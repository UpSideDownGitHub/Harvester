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
        if (!PhotonNetwork.IsMasterClient)
            return;

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

/// <summary>
/// Saves the current map data to the file.
/// </summary>
/// <remarks>
/// This method loads general and map save data, iterates through the placed objects, and records their positions and IDs in the map data.
/// It ensures no duplicate entries by using a list to track processed grid spaces. The method also updates the number of current spawns
/// and saves the modified map data.
/// </remarks>
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

/// <summary>
/// Determines the clicked object at the specified screen position.
/// </summary>
/// <param name="clickPos">The screen position of the click.</param>
/// <returns>The GameObject clicked, or null if no object is clicked.</returns>
/// <remarks>
/// This method converts the screen position to world position and then to grid position.
/// It checks if there is an object at the clicked grid position and returns the corresponding GameObject if found.
/// </remarks>
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

/// <summary>
/// RPC method to remove an object from the grid at the specified position.
/// </summary>
/// <param name="position">The world position of the object to remove.</param>
/// <remarks>
/// This method updates the navigation mesh, retrieves the grid position of the object, 
/// and removes the object's data from the placedObjects dictionary along with its associated grid spaces.
/// </remarks>
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

/// <summary>
/// RPC method to set spawned objects on the grid with the specified parameters.
/// </summary>
/// <param name="objectName">The name of the spawned object.</param>
/// <param name="recivedPositions">The array of received positions for the spawned object.</param>
/// <param name="ID">The unique ID of the spawned object.</param>
/// <remarks>
/// This method converts received positions to grid positions and updates the navigation mesh.
/// It then creates ObjectData instances for each grid position and adds them to the placedObjects dictionary.
/// </remarks>
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


/// <summary>
/// Spawns the specified object on the grid at the given position with rotation and associated grid positions.
/// </summary>
/// <param name="objectToSpawn">The object prefab to spawn.</param>
/// <param name="pos">The world position to spawn the object.</param>
/// <param name="rot">The rotation of the spawned object.</param>
/// <param name="gridPositions">The list of grid positions associated with the spawned object.</param>
/// <param name="ID">The unique ID of the spawned object.</param>
/// <remarks>
/// This method plays the placeItem sound effect, converts grid positions to object[], and instantiates the object on the network.
/// It also sets a unique name for all instances of the spawned object and synchronizes the spawned object's data across the network.
/// </remarks>
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

/// <summary>
/// Attempts to place an object on the grid at the specified position with the given ID.
/// </summary>
/// <param name="ID">The ID of the object to place.</param>
/// <param name="gridPosition">The grid position where the object is to be placed.</param>
/// <returns>True if the object is successfully placed, false otherwise.</returns>
/// <remarks>
/// This method checks if the specified grid position is within the bounds of the current area and above land.
/// It then calculates the grid positions occupied by the object and checks if any of them overlap with existing objects.
/// If the placement is valid, it calls the Spawn method to instantiate the object on the grid and synchronizes the data across the network.
/// </remarks>
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

/// <summary>
/// Attempts to place an object in the world at the specified position with the given ID.
/// </summary>
/// <param name="ID">The ID of the object to place.</param>
/// <param name="spawnPosition">The world position where the object is to be placed.</param>
/// <returns>True if the object is successfully placed, false otherwise.</returns>
/// <remarks>
/// This method converts the world position to grid position and checks if it is within the bounds of the current area and above land.
/// It then calculates the grid positions occupied by the object and checks if any of them overlap with existing objects.
/// If the placement is valid, it calls the Spawn method to instantiate the object in the world and synchronizes the data across the network.
/// </remarks>
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


/// <summary>
/// Retrieves the grid positions occupied by an object based on its starting grid position and size.
/// </summary>
/// <param name="gridPos">The starting grid position of the object.</param>
/// <param name="size">The size of the object in grid units.</param>
/// <returns>A list of grid positions occupied by the object.</returns>
/// <remarks>
/// This method generates a list of grid positions by iterating through the object's size in both the x and y directions
/// and calculating the positions relative to the starting grid position.
/// </remarks>
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

/// <summary>
/// Checks if any of the specified grid positions are already occupied by another object.
/// </summary>
/// <param name="positions">The list of grid positions to check.</param>
/// <returns>True if any position is occupied, false otherwise.</returns>
/// <remarks>
/// This method iterates through the list of grid positions and checks if any of them are present in the placedObjects dictionary,
/// indicating that another object is already occupying that position.
/// </remarks>
    public bool isAboveAnotherObject(List<Vector3Int> positions)
    {
        foreach (var pos in positions)
        {
            if (placedObjects.ContainsKey(pos))
                return true;
        }
        return false;
    }

/// <summary>
/// Checks if the specified grid position and object size are within the bounds of the current area.
/// </summary>
/// <param name="gridPos">The starting grid position of the object.</param>
/// <param name="size">The size of the object in grid units.</param>
/// <returns>True if the object can be placed within the area bounds, false otherwise.</returns>
/// <remarks>
/// This method iterates through the object's size in both the x and y directions, calculating positions relative to the starting grid position.
/// For each calculated position, it checks if it is within the bounds of any of the defined area borders.
/// The method returns true if all calculated positions are within the area bounds, indicating the object can be placed.
/// </remarks>
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

/// <summary>
/// Checks if the specified grid position is within the specified range in both x and y directions.
/// </summary>
/// <param name="pos">The grid position to check.</param>
/// <param name="xMin">The minimum x-coordinate of the range (default is -100).</param>
/// <param name="xMax">The maximum x-coordinate of the range (default is 100).</param>
/// <param name="yMin">The minimum y-coordinate of the range (default is -100).</param>
/// <param name="yMax">The maximum y-coordinate of the range (default is 100).</param>
/// <returns>True if the position is within the specified range, false otherwise.</returns>
/// <remarks>
/// This method checks if the x and y coordinates of the given position are within the specified range.
/// </remarks>
    bool inRange(Vector3Int pos, int xMin = -100, int xMax = 100, int yMin = -100, int yMax = 100) =>
        ((pos.x - xMin) * (pos.x - xMax) <= 0) && ((pos.y - yMin) * (pos.y - yMax) <= 0);
}

/// <summary>
/// Serializable class representing data for an object placed on the grid.
/// </summary>
[Serializable]
public class ObjectData
{
    public List<Vector3Int> gridSpaces;
    public int ID;
    public string objectName;

/// <summary>
/// Default constructor.
/// </summary>
    public ObjectData() { }

/// <summary>
/// Parameterized constructor to initialize object data.
/// </summary>
/// <param name="gridSpaces">List of grid positions occupied by the object.</param>
/// <param name="ID">The unique ID of the object.</param>
/// <param name="objectName">The name of the object.</param>
    public ObjectData(List<Vector3Int> gridSpaces, int ID, string objectName)
    {
        this.gridSpaces = gridSpaces;
        this.ID = ID;
        this.objectName = objectName;
    }
}