using FishNet.Object;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    public GameObject[] enemies;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            SpawnEnemies();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnEnemies()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            var pos = new Vector3(transform.position.x + i, transform.position.y, transform.position.z);
            var enemy = Instantiate(enemies[i], pos, Quaternion.identity);
            ServerManager.Spawn(enemy,base.LocalConnection);
        }
        
    }
}
