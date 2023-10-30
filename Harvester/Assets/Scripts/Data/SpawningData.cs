using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Data Objects/Nature Spawning")]
public class SpawnAreas : ScriptableObject
{
    public areaSpawns[] spawns;
}

[Serializable]
public struct areaSpawns
{
    public int[] itemsToSpawn;
    public int maxItems;
    public int currentItems;
    public float spawnTime;
    public float timeOfLastSpawn;
}