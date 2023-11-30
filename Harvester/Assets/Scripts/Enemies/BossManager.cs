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
        if (PhotonNetwork.IsMasterClient)
            return;

        // load the save data
        var currentData = SaveManager.instance.LoadGeneralSaveData();
        var save = SaveManager.instance.LoadMapSaveData();
        if (save.maps[currentData.mapID].section1Unlocked)
            areaBlockers[0].SetActive(false);
        if (save.maps[currentData.mapID].section2Unlocked)
            areaBlockers[1].SetActive(false);
        if (save.maps[currentData.mapID].section3Unlocked)
            areaBlockers[2].SetActive(false);
        if (save.maps[currentData.mapID].section4Unlocked)
            areaBlockers[3].SetActive(false);
    }

    public void BossKilled(int bossID)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        var currentData = SaveManager.instance.LoadGeneralSaveData();
        var save = SaveManager.instance.LoadMapSaveData();
        if (bossID == 0)
        {
            save.maps[currentData.mapID].section1Unlocked = true;
            areaBlockers[0].SetActive(false);
        }
        else if (bossID == 1)
        {
            save.maps[currentData.mapID].section2Unlocked = true;
            areaBlockers[1].SetActive(false);
        }
        else if (bossID == 2)
        {
            save.maps[currentData.mapID].section3Unlocked = true;
            areaBlockers[2].SetActive(false);
        }
        else if (bossID == 3)
        {
            save.maps[currentData.mapID].section4Unlocked = true;
            areaBlockers[3].SetActive(false);
        }
        SaveManager.instance.SaveMapData(save);
    }


    public bool isBossAlive()
    {
        return GameObject.FindGameObjectWithTag(bossTag) != null ? true : false;
    }
}
