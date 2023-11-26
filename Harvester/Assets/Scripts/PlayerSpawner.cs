using Photon.Pun;
using Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject player;
    public Transform spawnPos;

    public void Start()
    {
        PhotonNetwork.Instantiate(player.name, spawnPos.position, Quaternion.identity, 0);
    }
}
