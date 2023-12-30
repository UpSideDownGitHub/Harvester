using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NavMeshPlus.Components;
using Photon.Pun;
using System.Security.Policy;

public class BossManager : MonoBehaviour
{
    public string bossTag;
    public GameObject[] areaBlockers;

    public PhotonView photonView;

    [Header("Final Boss Killed Menu")]
    public float showTime;
    public GameObject gameBeatenScreen;

    /// <summary>
    /// Start method that checks if the current player is the master client in a PhotonNetwork game.
    /// If true, it loads save data and unlocks corresponding map sections for all players using PhotonRPC calls.
    /// </summary>
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

    /// <summary>
    /// PhotonRPC method to unlock a specific map area by deactivating its corresponding blocker GameObject.
    /// </summary>
    /// <param name="areaID">The identifier of the area to be unlocked.</param>
    [PunRPC]
    public void UnlockArea(int areaID)
    {
        areaBlockers[areaID].SetActive(false);
    }

    /// <summary>
    /// Handles the event of a boss being killed.
    /// </summary>
    /// <param name="bossID">The identifier of the defeated boss.</param>
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
        else if (bossID == 4)
        {
            StartCoroutine(ShowMenu());
        }
        SaveManager.instance.SaveMapData(save);
    }

    /// <summary>
    /// Coroutine to display the game beaten screen and hide it after a specified time delay.
    /// </summary>
    /// <returns>An IEnumerator used for coroutine functionality.</returns>
    public IEnumerator ShowMenu()
    {
        gameBeatenScreen.SetActive(true);
        yield return new WaitForSeconds(showTime);
        gameBeatenScreen.SetActive(false);
    }

    /// <summary>
    /// Checks if a boss is currently alive in the scene.
    /// </summary>
    /// <returns>True if a boss is alive, false otherwise.</returns>
    public bool isBossAlive()
    {
        return GameObject.FindGameObjectWithTag(bossTag) != null ? true : false;
    }
}
