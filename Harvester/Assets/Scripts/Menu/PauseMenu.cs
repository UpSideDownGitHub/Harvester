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

/// <summary>
/// Handles the update logic for the pause menu based on user input.
/// </summary>
/// <remarks>
/// This method checks for the pressing of the Escape key and toggles the visibility of the pause menu accordingly.
/// If the current menu is close, it closes the pause menu; otherwise, it opens the pause menu if it can be opened.
/// </remarks>
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

/// <summary>
/// Handles the button press event for resuming the game from the pause menu.
/// </summary>
/// <remarks>
/// This method toggles the pause status, sets the menuOpen to NOTHING, and adjusts the visibility of the pause menu accordingly.
/// </remarks>
    public void ResumePressed()
    {
        MenuManager.menuOpen = MenuID.NOTHING;
        paused = !paused;
        pauseMenuObject.SetActive(paused);
    }
/// <summary>
/// Handles the button press event for saving game data.
/// </summary>
/// <remarks>
/// This method checks if the current player is the master client in the PhotonNetwork.
/// If true, it calls the SaveMapData method of the GridManager and the SavePlayerData method of the Inventory to save map and player data, respectively.
/// If false, it only calls the SavePlayerData method to save player data.
/// </remarks>
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
/// <summary>
/// Handles the button press event for saving game data and exiting to the main menu.
/// </summary>
/// <remarks>
/// This method calls the SavePressed method to save game data and then invokes the MainMenuPressed method to navigate to the main menu.
/// </remarks>
    public void SaveExitPressed()
    {
        SavePressed();
        MainMenuPressed();
    }
/// <summary>
/// Handles the button press event for navigating to the main menu.
/// </summary>
/// <remarks>
/// This method sets the menuOpen to NOTHING and instructs PhotonNetwork to leave the current room.
/// </remarks>
    public void MainMenuPressed()
    {
        MenuManager.menuOpen = MenuID.NOTHING;
        PhotonNetwork.LeaveRoom();
    }
/// <summary>
/// Overrides the method called when the master client is switched in the Photon network.
/// </summary>
/// <param name="newMasterClient">The new master client in the Photon network.</param>
/// <remarks>
/// This method invokes the MainMenuPressed method when the master client is switched.
/// </remarks>
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        MainMenuPressed();
    }
/// <summary>
/// Overrides the method called when connected to the Photon network master server.
/// </summary>
/// <remarks>
/// This method instructs PhotonNetwork to join the lobby upon successfully connecting to the master server.
/// </remarks>
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
/// <summary>
/// Overrides the method called when the player successfully joins the Photon lobby.
/// </summary>
/// <remarks>
/// This method loads the "Lobby" scene using SceneManager upon successfully joining the lobby.
/// </remarks>
    public override void OnJoinedLobby()
    {
        SceneManager.LoadScene("Lobby");
    }
/// <summary>
/// Handles the button press event for exiting the game.
/// </summary>
/// <remarks>
/// This method instructs PhotonNetwork to leave the current room and exits the application.
/// </remarks>
    public void ExitPressed()
    {
        PhotonNetwork.LeaveRoom();
        Application.Quit();
    }
}
