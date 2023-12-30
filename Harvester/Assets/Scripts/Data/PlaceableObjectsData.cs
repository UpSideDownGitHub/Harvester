using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// ScriptableObject class representing a collection of PlaceableObjects.
/// </summary>
[CreateAssetMenu(menuName= "Data Objects/Placeable Object")]
public class PlaceableObjectsData : ScriptableObject
{
    public List<PlaceableObjects> placeables;
}
/// <summary>
/// Serializable structure defining a placeable object with a name, ID, size, and associated prefab.
/// </summary>
[Serializable]
public struct PlaceableObjects
{
    public string placeableName;
    public int placeableID;
    public Vector2Int size;
    public GameObject prefab;
}
