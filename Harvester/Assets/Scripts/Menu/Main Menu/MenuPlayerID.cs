using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuPlayerID : MonoBehaviour
{
    public int ID;
    public TMP_Text playerName;
    public Image[] heartIcons;
    public LobbyManager lobbyManager;
    public Button button;

/// <summary>
/// Sets up the PlayerSelectionButton with the specified LobbyManager.
/// </summary>
/// <param name="manager">The LobbyManager instance to associate with the button.</param>
/// <remarks>
/// This method assigns the given LobbyManager to the lobbyManager field and adds a listener to the button's onClick event,
/// invoking the PlayerSelected method of the LobbyManager when the button is clicked with the current PlayerSelectionButton's ID and reference.
/// </remarks>
    public void Setup(LobbyManager manager)
    {
        lobbyManager = manager;
        button.onClick.AddListener(() => lobbyManager.PlayerSelected(ID, this));
    }
}
