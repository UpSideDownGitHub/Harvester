using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// ScriptableObject class representing a collection of ToolObjects.
/// </summary>
[CreateAssetMenu(menuName= "Data Objects/Tool Object")]
public class ToolObjectData : ScriptableObject
{
    public List<ToolObjects> Tools;
}
/// <summary>
/// Serializable structure defining a tool with various properties.
/// </summary>
[Serializable]
public struct ToolObjects
{
    public string[] toolName;
    public int[] toolID;
    public float[] toolUseSpeed;
    public float[] toolDamage;
    public Sprite[] toolIcons;
    public ToolType type;

}
/// <summary>
/// Enumeration representing different types of tools.
/// </summary>
[Serializable]
public enum ToolType
{
    PICKAXE,
    AXE,
    SWORD
}
