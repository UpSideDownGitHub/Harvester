using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviourPunCallbacks
{
    public bool paused = false;
    public GameObject pauseMenuObject;

    [Header("Saving")]
    public GridManager gridManager;
    public Inventory inventory;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            paused = !paused;
            pauseMenuObject.SetActive(paused);
        }
    }

    public void ResumePressed()
    {
        paused = !paused;
        pauseMenuObject.SetActive(paused);
    }
    public void SavePressed()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            gridManager.SaveMapData();
            inventory.SavePlayerData();
        }
        else
        {
            inventory.SavePlayerData();
        }
    }
    public void SaveExitPressed()
    {
        SavePressed();
        MainMenuPressed();
    }
    public void MainMenuPressed()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        MainMenuPressed();
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        SceneManager.LoadScene("Lobby");
    }
    public void ExitPressed()
    {
        PhotonNetwork.LeaveRoom();
        Application.Quit();
    }
}
