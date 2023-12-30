using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


/// <summary>
/// ScriptableObject class representing data for nature spawning in specific areas.
/// </summary>
[CreateAssetMenu(menuName = "Data Objects/Nature Spawning")]
public class SpawnAreas : ScriptableObject
{
    public areaSpawns[] spawns;
}

/// <summary>
/// Serializable structure defining the spawning properties for a specific area.
/// </summary>
[Serializable]
public struct areaSpawns
{
    public int[] itemsToSpawn;
    public int maxItems;
    public float spawnTime;
    public float timeOfLastSpawn;
}