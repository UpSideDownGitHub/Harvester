using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public GameObject StartMenu;
    public GameObject LobbyMenu;
    

/// <summary>
/// Handles the button press event for starting the game.
/// </summary>
/// <remarks>
/// This method sets the StartMenu to inactive and activates the LobbyMenu.
/// </remarks>
    public void StartPressed()
    {
        StartMenu.SetActive(false);
        LobbyMenu.SetActive(true);
    }
/// <summary>
/// Handles the button press event for going back to the start menu.
/// </summary>
/// <remarks>
/// This method sets the StartMenu to active and deactivates the LobbyMenu.
/// </remarks>
    public void BackPressed()
    {
        StartMenu.SetActive(true);
        LobbyMenu.SetActive(false);
    }
}
