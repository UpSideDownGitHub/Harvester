using JetBrains.Annotations;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson.PunDemos;

public class MiscManager : MonoBehaviour
{
    public GameObject deathUI;

    public GameObject interactUI;

    public GameObject GameBeatenUI;

    public int currentPlayers;
    public int currentAlivePlayers;

/// <summary>
/// Sets the number of alive and total players, updating all clients.
/// </summary>
/// <param name="alive">Indicates whether to update alive players count.</param>
/// <param name="add">Indicates whether to increment or decrement the count.</param>
/// <remarks>
/// This method is a PunRPC (Photon Unity Networking Remote Procedure Call) that updates the counts of alive or total players based on the provided parameters.
/// It is intended to be called by the master client, and the updated counts are then broadcasted to all clients using RPCs.
/// </remarks>
    [PunRPC]
    public void SetPlayers(bool alive, bool add)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonView view = PhotonView.Get(this);
            if (alive)
            {
                currentAlivePlayers += add ? 1 : -1;
                view.RPC("SetAlivePlayers", RpcTarget.All, currentAlivePlayers);                
            }
            else
            { 
                currentPlayers += add ? 1 : -1;
                view.RPC("SetPlayers", RpcTarget.All, currentPlayers);
            }
        }
    }

/// <summary>
/// Sets the number of alive players to the specified value.
/// </summary>
/// <param name="alivePlayers">The new value for the number of alive players.</param>
/// <remarks>
/// This method is a PunRPC (Photon Unity Networking Remote Procedure Call) that sets the number of alive players to the specified value.
/// It is intended to be called by the master client and updates the alive players count for all clients.
/// </remarks>
    [PunRPC]
    public void SetAlivePlayers(int alivePlayers)
    {
        this.currentAlivePlayers = alivePlayers;
    }

/// <summary>
/// Sets the number of total players to the specified value.
/// </summary>
/// <param name="players">The new value for the number of total players.</param>
/// <remarks>
/// This method is a PunRPC (Photon Unity Networking Remote Procedure Call) that sets the number of total players to the specified value.
/// It is intended to be called by the master client and updates the total players count for all clients.
/// </remarks>
    [PunRPC]
    public void SetPlayers(int players)
    {
        this.currentPlayers = players;
    }
}
