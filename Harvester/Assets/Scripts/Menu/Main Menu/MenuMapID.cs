using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuMapID : MonoBehaviour
{
    public int ID;
    public TMP_Text mapName;
    public Image mapIcon;
    public LobbyManager lobbyManager;
    public Button button;

/// <summary>
/// Sets up the MapSelectionButton with the specified LobbyManager.
/// </summary>
/// <param name="manager">The LobbyManager instance to associate with the button.</param>
/// <remarks>
/// This method assigns the given LobbyManager to the lobbyManager field and adds a listener to the button's onClick event,
/// invoking the MapSelected method of the LobbyManager when the button is clicked with the current MapSelectionButton's ID and reference.
/// </remarks>
    public void Setup(LobbyManager manager)
    {
        lobbyManager = manager;
        button.onClick.AddListener(() => lobbyManager.MapSelected(ID, this));
    }
}
