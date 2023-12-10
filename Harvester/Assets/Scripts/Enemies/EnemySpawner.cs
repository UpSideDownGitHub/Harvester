using Newtonsoft.Json;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Data")]
    public GameObject[] enemies;
    public GridManager grid;

    [Header("Spawning")]
    public float spawnTime;
    [Range(1f, 0f)]
    public float spawnChance;
    private float _timeSinceLastSpawn;
    public float spawnSearchRadius = 5;

    public void Start()
    {
        PhotonView photonView = PhotonView.Get(this);
        if (!photonView.IsMine)
            gameObject.GetComponent<EnemySpawner>().enabled = false;
    }

    public void Update()
    {
        if (Time.time > spawnTime + _timeSinceLastSpawn && Random.value > spawnChance)
        {
            _timeSinceLastSpawn = Time.time;
            // check all available ares from the savedata
            var currentData = SaveManager.instance.LoadGeneralSaveData();
            var save = SaveManager.instance.LoadMapSaveData();
            int unlockedAreas = 1;
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
            PhotonNetwork.Instantiate("Enemies/" + enemies[selectedArea].name, navHit.position, Quaternion.identity, 0);
        }
    }
}
