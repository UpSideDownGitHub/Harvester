using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName= "Data Objects/Tool Object")]
public class ToolObjectData : ScriptableObject
{
    public List<ToolObjects> Tools;
}
[Serializable]
public struct ToolObjects
{
    public string[] toolName;
    public int[] toolID;
    public float[] toolUseSpeed;
    public float[] toolDamage;
    public Sprite[] toolIcons;
    public int level;
    public ToolType type;

}
[Serializable]
public enum ToolType
{
    PICKAXE,
    AXE,
    SWORD
}
