using FishNet.Object;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemSpawner : NetworkBehaviour
{
    public SpawnAreas spawns;
    public GridManager gridManager;
    public GameObject temp;

    public int maxSpawnAttempts;

    public override void OnStartClient()
    {
        base.OnStartClient();
        for (int i = 0; i < spawns.spawns.Length; i++)
        {
            spawns.spawns[i].currentItems = 0;
            spawns.spawns[i].timeOfLastSpawn = 0;
        }

        if (base.IsOwner)
            gameObject.GetComponent<ItemSpawner>().enabled = false;
    }

    public void Update()
    {
        for (int i = 0; i < spawns.spawns.Length; i++)
        {
            if (spawns.spawns[i].currentItems >= spawns.spawns[i].maxItems)
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
                    spawns.spawns[i].currentItems++;
                    break;
                }
            }
        }
    }
}