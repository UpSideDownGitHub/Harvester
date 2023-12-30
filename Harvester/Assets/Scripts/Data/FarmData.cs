using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// ScriptableObject class representing a collection of Farm objects.
/// </summary>
[CreateAssetMenu(menuName = "Data Objects/Farm")]
public class FarmData : ScriptableObject
{
    public List<Farm> Farms;
}

/// <summary>
/// Serializable structure defining a farm with a name, ID, and an array of items with associated chances.
/// </summary>
[Serializable]
public struct Farm
{
    public string farmName;
    public int farmID;
    public ItemChance[] items;
}

/// <summary>
/// Serializable structure defining an item with an associated chance.
/// </summary>
[Serializable]
public struct ItemChance
{
    public Item item;
    [Range(0,1)]
    public float chance;
}
