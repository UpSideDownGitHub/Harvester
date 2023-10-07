using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName= "Data Objects/Placeable Object")]
public class PlaceableObjectsData : ScriptableObject
{
    public List<PlaceableObjects> placeables;
}
[Serializable]
public struct PlaceableObjects
{
    public string placeableName;
    public int placeableID;
    public Vector2Int size;
    public GameObject prefab;
}
