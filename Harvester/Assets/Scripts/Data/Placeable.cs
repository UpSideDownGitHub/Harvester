using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

/// <summary>
/// ScriptableObject class representing a placeable object in the game.
/// </summary>
[CreateAssetMenu(menuName = "Data Objects/Placeable")]
public class Placeable : ScriptableObject
{
    public string placeableName;
    public int placeableID;
    public float health;
    public ToolType breakType;
    public itemCount[] drops;
}
