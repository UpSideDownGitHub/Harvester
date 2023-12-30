using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// ScriptableObject class representing a collection of ConsumableObjects.
/// </summary>
[CreateAssetMenu(menuName= "Data Objects/Consumable Object")]
public class ConsumableObjectData : ScriptableObject
{
    public List<ConsumableObjects> consumables;
}
/// <summary>
/// Serializable structure defining a consumable object with a name, ID, and stamina increase value.
/// </summary>
[Serializable]
public struct ConsumableObjects
{
    public string consumableName;
    public int consumableID;
    public float staminaIncrease;
}
