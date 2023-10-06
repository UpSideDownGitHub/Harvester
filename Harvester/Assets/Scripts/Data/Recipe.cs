
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Data Objects/Recipies")]
public class Recipe : ScriptableObject
{
    public string recipeName;
    public int ID;
    public float time;
    [SerializeField]
    [Tooltip("ID & Amount")]
    public itemCount[] materials;
    [Tooltip("ID of the given Item & the amount given")]
    public itemCount produces;
}
[Serializable]
public struct itemCount
{
    public Item item;
    public int count;
}
