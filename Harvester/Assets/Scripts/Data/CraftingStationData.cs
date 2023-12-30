using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// ScriptableObject class representing a collection of CraftingStation objects.
/// </summary>
[CreateAssetMenu(menuName = "Data Objects/Crafting Station")]
public class CraftingStationData : ScriptableObject
{
    public List<CraftingStation> stations;
}
/// <summary>
/// Serializable structure defining a crafting station with a name, ID, and an array of associated recipes.
/// </summary>
[Serializable]
public struct CraftingStation
{
    public string stationName;
    public int stationID;
    public Recipe[] recipies;
}
