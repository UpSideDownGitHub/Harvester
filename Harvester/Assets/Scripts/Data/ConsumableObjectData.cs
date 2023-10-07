using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName= "Data Objects/Consumable Object")]
public class ConsumableObjectData : ScriptableObject
{
    public List<ConsumableObjects> consumables;
}
[Serializable]
public struct ConsumableObjects
{
    public string consumableName;
    public int consumableID;
    public float staminaIncrease;
}
