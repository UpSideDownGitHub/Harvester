using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Grid worldGrid;
    public GameObject tempObject;

    [Serializable]
    public struct borders
    {
        [Tooltip("Top Left")]
        public Vector3Int TL;
        [Tooltip("Bottom Right")]
        public Vector3Int BR;
    }
    public borders[] areaBorders;

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
}
