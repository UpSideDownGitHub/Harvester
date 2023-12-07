using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NavMeshPlus.Components;
using Photon.Pun;

public class BossManager : MonoBehaviour
{
    public string bossTag;
    public GameObject[] areaBlockers;

    public PhotonView photonView;

    public void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        PhotonView photonView = PhotonView.Get(this);

        // load the save data
        var currentData = SaveManager.instance.LoadGeneralSaveData();
        var save = SaveManager.instance.LoadMapSaveData();
        if (save.maps[currentData.mapID].section1Unlocked)
        {
            photonView.RPC("UnlockArea", RpcTarget.All, 0);
        }
        if (save.maps[currentData.mapID].section2Unlocked)
        {
            photonView.RPC("UnlockArea", RpcTarget.All, 1);
        }
        if (save.maps[currentData.mapID].section3Unlocked)
        {
            photonView.RPC("UnlockArea", RpcTarget.All, 2);
        }
        if (save.maps[currentData.mapID].section4Unlocked)
        {
            photonView.RPC("UnlockArea", RpcTarget.All, 3);
        }
    }

    [PunRPC]
    public void UnlockArea(int areaID)
    {
        areaBlockers[areaID].SetActive(false);
    }

    public void BossKilled(int bossID)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        PhotonView photonView = PhotonView.Get(this);

        var currentData = SaveManager.instance.LoadGeneralSaveData();
        var save = SaveManager.instance.LoadMapSaveData();
        if (bossID == 0)
        {
            save.maps[currentData.mapID].section1Unlocked = true;
            photonView.RPC("UnlockArea", RpcTarget.All, 0);
        }
        else if (bossID == 1)
        {
            save.maps[currentData.mapID].section2Unlocked = true;
            photonView.RPC("UnlockArea", RpcTarget.All, 1);
        }
        else if (bossID == 2)
        {
            save.maps[currentData.mapID].section3Unlocked = true;
            photonView.RPC("UnlockArea", RpcTarget.All, 2);
        }
        else if (bossID == 3)
        {
            save.maps[currentData.mapID].section4Unlocked = true;
            photonView.RPC("UnlockArea", RpcTarget.All, 3);
        }
        SaveManager.instance.SaveMapData(save);
    }


    public bool isBossAlive()
    {
        return GameObject.FindGameObjectWithTag(bossTag) != null ? true : false;
    }
}
