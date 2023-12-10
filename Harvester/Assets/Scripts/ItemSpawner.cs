using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemSpawner : MonoBehaviour
{
    public SpawnAreas spawns;
    public GridManager gridManager;
    public GameObject temp;

    public int maxSpawnAttempts;

    public int[] currentSpawns;


    public void Start()
    {
        var mapSaveData = SaveManager.instance.LoadMapSaveData();
        var pickedData = SaveManager.instance.LoadGeneralSaveData();
        currentSpawns = mapSaveData.maps[pickedData.mapID].spawns;

        for (int i = 0; i < spawns.spawns.Length; i++)
        {
            spawns.spawns[i].timeOfLastSpawn = 0;
        }
        PhotonView view = PhotonView.Get(this);
        if (!view.IsMine)
            gameObject.GetComponent<ItemSpawner>().enabled = false;
    }

    public void Update()
    {
        for (int i = 0; i < spawns.spawns.Length; i++)
        {
            if (currentSpawns[i] >= spawns.spawns[i].maxItems)
                continue;
            if (Time.time < spawns.spawns[i].spawnTime + spawns.spawns[i].timeOfLastSpawn)
                continue;

            spawns.spawns[i].timeOfLastSpawn = Time.time;
            borders spawnArea = gridManager.areaBorders[i];
            var randomObject = Random.Range(0, spawns.spawns[i].itemsToSpawn.Length);
            for (int j = 0; j < maxSpawnAttempts; j++)
            {
                var randomPos = new Vector3(Random.Range(spawnArea.TL.x, spawnArea.BR.x), Random.Range(spawnArea.BR.y, spawnArea.TL.y), 0);
                bool placed = gridManager.placeObjectWorld(spawns.spawns[i].itemsToSpawn[randomObject], randomPos);
                if (placed)
                {
                    currentSpawns[i]++;
                    break;
                }
            }
        }
    }
}