
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Data Objects/Recipies")]
public class RecipeData : ScriptableObject
{
    public List<Recipe> recipies;
}
[Serializable]
public struct Recipe
{
    public string recipeName;
    public int ID;
    public float time;
    [Tooltip("ID & Amount")]
    public Dictionary<Item, int> materials;
    public Item item;
    public int count;
}
