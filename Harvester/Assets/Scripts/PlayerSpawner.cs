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

/// <summary>
/// Initializes the local player by instantiating it in the networked environment and setting its name.
/// </summary>
/// <remarks>
/// This method is called when the local player starts. It instantiates the player object in the networked environment
/// at the specified spawn position and sets its name using a Photon RPC (Remote Procedure Call).
/// </remarks>
    public void Start()
    {
        var tempPlayer = PhotonNetwork.Instantiate(player.name, spawnPos.position, Quaternion.identity, 0);
        PhotonView view = PhotonView.Get(tempPlayer);
        view.RPC("SetName", RpcTarget.All, tempPlayer.GetInstanceID().ToString());
    }
}
