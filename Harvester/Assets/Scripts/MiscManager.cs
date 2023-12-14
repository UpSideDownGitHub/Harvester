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

    [PunRPC]
    public void SetAlivePlayers(int alivePlayers)
    {
        this.currentAlivePlayers = alivePlayers;
    }

    [PunRPC]
    public void SetPlayers(int players)
    {
        this.currentPlayers = players;
    }
}
