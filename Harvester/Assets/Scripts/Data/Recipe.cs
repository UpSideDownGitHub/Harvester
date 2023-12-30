
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// ScriptableObject class representing a crafting recipe.
/// </summary>
[CreateAssetMenu(menuName = "Data Objects/Recipies")]
public class Recipe : ScriptableObject
{
    public string recipeName;
    public int recipeID;
    public float time;
    [SerializeField]
    [Tooltip("ID & Amount")]
    public itemCount[] materials;
    [Tooltip("ID of the given Item & the amount given")]
    public itemCount produces;
}
/// <summary>
/// Serializable structure defining an item and its associated count.
/// </summary>
[Serializable]
public struct itemCount
{
    public Item item;
    public int count;
}
