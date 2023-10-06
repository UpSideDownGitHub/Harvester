using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Data Objects/Crafting Station")]
public class CraftingStationData : ScriptableObject
{
    public List<CraftingStation> stations;
}
[Serializable]
public struct CraftingStation
{
    public string stationName;
    public int ID;
    public Recipe[] recipies;
}
