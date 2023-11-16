using FishNet.Object;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : NetworkBehaviour
{
    [Header("Data")]
    public GameObject[] enemies;
    public GridManager grid;
    public PickedData currentData;

    [Header("Spawning")]
    public float spawnTime;
    private float _timeSinceLastSpawn;
    public float spawnSearchRadius = 5;


    public void Update()
    {
        if (Time.time > spawnTime + _timeSinceLastSpawn)
        {
            _timeSinceLastSpawn = Time.time;
            // check all available ares from the savedata
            var save = SaveManager.instance.LoadMapSaveData();
            int unlockedAreas = 1;
            Debug.Log("Current Map: " + currentData.mapID);
            Debug.Log("Total Maps: " + save.maps.Count);
            unlockedAreas += save.maps[currentData.mapID].section1Unlocked ? 1 : 0;
            unlockedAreas += save.maps[currentData.mapID].section2Unlocked ? 1 : 0;
            unlockedAreas += save.maps[currentData.mapID].section3Unlocked ? 1 : 0;
            unlockedAreas += save.maps[currentData.mapID].section4Unlocked ? 1 : 0;

            // get spawn area
            int selectedArea = Random.Range(0, unlockedAreas);
            borders spawnArea = grid.areaBorders[selectedArea];

            // get randomPosition
            var randomPos = new Vector3(Random.Range(spawnArea.TL.x, spawnArea.BR.x), Random.Range(spawnArea.BR.y, spawnArea.TL.y), 0);
            NavMeshHit navHit;
            var navMeshpos = NavMesh.SamplePosition(randomPos, out navHit, spawnSearchRadius, -1);

            // spawn enemy in given area
            SpawnEnemies(selectedArea, navHit.position);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void SpawnEnemies(int ID, Vector3 pos)
    {
        var enemy = Instantiate(enemies[ID], pos, Quaternion.identity);
        ServerManager.Spawn(enemy, base.LocalConnection);
    }
}
