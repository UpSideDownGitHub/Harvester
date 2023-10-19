using System;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Grid worldGrid;
    public GameObject tempObject;

    [Header("Grid Information")]
    //public List<GameObject> spawnedObjects = new();
    public Dictionary<Vector3Int, ObjectData> placedObjects = new();

    [Header("Data")]
    public PlaceableObjectsData placeables;


    [Serializable]
    public struct borders
    {
        [Tooltip("Top Left")]
        public Vector3Int TL;
        [Tooltip("Bottom Right")]
        public Vector3Int BR;
    }
    public borders[] areaBorders;

    // THIS IS JUST FOR TESTING PURPOSES SO PLEASE DELETE
    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // try and spawn an object
            //placeObject(0, Input.mousePosition);

        }
    }

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
    public void RemoveObject(Vector3 position)
    {
        var gridPosition = worldGrid.WorldToCell(position);
        var data = placedObjects[gridPosition];
        for (int i = 0; i < data.gridSpaces.Count; i++)
        {
            placedObjects.Remove(data.gridSpaces[i]);
        }
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

        var spawnedObject = Instantiate(placeables.placeables[ID].prefab, worldGrid.CellToWorld(gridPosition), Quaternion.identity);

        foreach (var pos in gridPositions)
        {
            var data = new ObjectData(gridPositions, ID, spawnedObject);
            placedObjects[pos] = data;
        }
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

public class ObjectData
{
    public List<Vector3Int> gridSpaces;
    public int ID;
    public GameObject spawnedObject;

    public ObjectData(List<Vector3Int> gridSpaces, int ID, GameObject spawnedObject)
    {
        this.gridSpaces = gridSpaces;
        this.ID = ID;
        this.spawnedObject = spawnedObject;
    }
}