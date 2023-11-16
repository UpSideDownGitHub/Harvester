using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using NavMeshPlus.Components;

public class BossManager : NetworkBehaviour
{
    public string bossTag;
    public GameObject[] areaBlockers;
    public PickedData currentData;

    public void Start()
    {
        // load the save data
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
