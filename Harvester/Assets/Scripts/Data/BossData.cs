using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// ScriptableObject class representing a collection of Boss objects.
/// </summary>
[CreateAssetMenu(menuName = "Data Objects/Boss")]
public class BossData : ScriptableObject
{
    public List<Boss> bosses;
}

/// <summary>
/// Represents a serializable structure defining a game boss with a name, ID, and associated GameObject.
/// </summary>
[Serializable]
public struct Boss
{
    public string bossName;
    public int bossID;
    public GameObject bossObject;
}
