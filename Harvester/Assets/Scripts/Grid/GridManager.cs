using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Grid worldGrid;
    public GameObject tempObject;

    [Header("Grid Information")]
    public List<GameObject> spawnedObjects = new();
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

    // THIS IS JUST FOR TESTING PURPOSES
    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // try and spawn an object
            placeObject(0, Input.mousePosition);

        }
    }

    public void placeObject(int ID, Vector3 clickPosition)
    {
        // get the position of the mouse
        var worldPosision = Camera.main.ScreenToWorldPoint(clickPosition);
        var gridPosition = worldGrid.WorldToCell(worldPosision);

        // check if the clicked posision is within the bounds of the current area
        if (!isAboveLand(gridPosition, placeables.objects[ID].size))
        {
            print("Error Not Above Land");
            return;
        }

        // need to check if there is room to spawn the object
        Instantiate(placeables.objects[ID].prefab, worldGrid.CellToWorld(gridPosition), Quaternion.identity);
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


    /*
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < areaBorders.Length; i++)
        {
            Instantiate(tempObject, worldGrid.CellToWorld(areaBorders[i].TL), Quaternion.identity);
            Instantiate(tempObject, worldGrid.CellToWorld(areaBorders[i].BR), Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        

        if (Input.GetMouseButtonDown(0))
        {
            var mousePosition = Input.mousePosition;
            var worldPosision = Camera.main.ScreenToWorldPoint(mousePosition);
            var gridPosition = worldGrid.WorldToCell(worldPosision);
            print("CELL ->" + gridPosition);
            Instantiate(tempObject, worldGrid.CellToWorld(gridPosition), Quaternion.identity);
        }
    }
    */

}

public class ObjectData
{
    public List<Vector3Int> gridSpaces;
    public int ID;
    public int objectIndex;

    public ObjectData(List<Vector3Int> gridSpaces, int ID, int objectIndex)
    {
        this.gridSpaces = gridSpaces;
        this.ID = ID;
        this.objectIndex = objectIndex;
    }
}