using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviourPunCallbacks
{
    public bool paused = false;
    public GameObject pauseMenuObject;

    [Header("Saving")]
    public GridManager gridManager;
    public Inventory inventory;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip closeMenu;
    public AudioClip openMenu;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (MenuManager.IsCurrentMenuClose(MenuID.PAUSE))
            {
                // Close
                audioSource.PlayOneShot(closeMenu);
                paused = false;
                pauseMenuObject.SetActive(paused);
            }
            else if (MenuManager.CanOpenMenuSet(MenuID.PAUSE))
            {
                // Open
                audioSource.PlayOneShot(openMenu);
                paused = true;
                pauseMenuObject.SetActive(paused);
            }
        }
    }

    public void ResumePressed()
    {
        MenuManager.menuOpen = MenuID.NOTHING;
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
        MenuManager.menuOpen = MenuID.NOTHING;
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
