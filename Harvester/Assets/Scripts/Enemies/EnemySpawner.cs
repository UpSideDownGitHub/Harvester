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

/// <summary>
/// Initialization method called when the object is started.
/// </summary>
/// <remarks>
/// This method retrieves the PhotonView component for network synchronization.
/// If the current instance is not the owner (IsMine is false), it disables the EnemySpawner component on the object.
/// </remarks>
    public void Start()
    {
        PhotonView photonView = PhotonView.Get(this);
        if (!photonView.IsMine)
            gameObject.GetComponent<EnemySpawner>().enabled = false;
    }

/// <summary>
/// Update method called each frame to manage enemy spawning behavior.
/// </summary>
/// <remarks>
/// This method checks if it's time to spawn a new enemy based on spawn time and chance.
/// It then calculates the number of unlocked areas from the saved data and selects a random area to spawn the enemy.
/// The enemy is instantiated within the selected area with a random position on the NavMesh.
/// </remarks>
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
