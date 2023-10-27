using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Data Objects/Farm")]
public class FarmData : ScriptableObject
{
    public List<Farm> Farms;
}
[Serializable]
public struct Farm
{
    public string farmName;
    public int farmID;
    public ItemChance[] items;
}
[Serializable]
public struct ItemChance
{
    public Item item;
    [Range(0,1)]
    public float chance;
}
