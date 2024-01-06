using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

/* This class manages the lobby of the game allowing the user to create new maps and players as well as 
allowing them to create, join and view lobbys*/
public class LobbyManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField roomInput;
    public GameObject lobbyPanel;
    public GameObject roomPanel;
    public TMP_Text roomName;

    [Header("Rooms")]
    public GameObject roomItem;
    private Dictionary<string, RoomItem> cachedRoomList = new Dictionary<string, RoomItem>();
    public Transform spawnArea;
    public float roomListUpdateDelay;
    private float _timeOfLastUpdate;
    public GameObject startButton;

    [Header("Players")]
    public List<PlayerItem> currentPlayers = new List<PlayerItem>();
    public GameObject playerPrefab;
    public Transform playerSpawnArea;

    [Header("Player & Map Selection")]
    public Transform playerIconSpawnPosition;
    public Transform mapIconSpawnPosition;
    public GameObject menuPlayerObject;
    public GameObject menuMapObject;

    public Sprite heartIcon;
    public Sprite emptyHeartIcon;
    public Sprite[] mapIcons;

    public List<GameObject> loadedPlayers = new();
    public List<GameObject> loadedMaps = new();

    public GameObject playerCreationUI;
    public GameObject mapCreationUI;

    public TMP_InputField playerNameEntry;
    public TMP_InputField mapNameEntry;


    [Header("Current Selected")]
    public int currentSelectedPlayer = -1;
    public int currentSelectedMap = -1;
    public Color selectedColour;
    public Color defaultColour;
    public Image currentSelectedPlayerImage;
    public Image currentSelectedMapImage;
    public Image previousSelectedPlayerImage;
    public Image previousSelectedMapImage;


    [Header("Menus")]
    public GameObject lobbyCanvas;
    public GameObject menuCanvas;
    public GameObject tutorialCanvas;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    /// <summary>
    /// The StartPressed function plays a click sound, activates the lobby canvas, and deactivates the
    /// menu canvas.
    /// </summary>
    public void StartPressed()
    {
        audioSource.PlayOneShot(clickSound);
        lobbyCanvas.SetActive(true);
        menuCanvas.SetActive(false);
    }

    /// <summary>
    /// The CloseLobbyPressed function plays a click sound, deactivates the lobby canvas, and activates the
    /// menu canvas.
    /// </summary>
    public void CloseLobbyPressed()
    {
        audioSource.PlayOneShot(clickSound);
        lobbyCanvas.SetActive(false);
        menuCanvas.SetActive(true);
    }

    /// <summary>
    /// The TutorialPressed function plays a click sound, deactivates the menu canvas, and activates the
    /// tutorial Canvas.
    /// </summary>
    public void TutorialPressed()
    {
        audioSource.PlayOneShot(clickSound);
        menuCanvas.SetActive(false);
        tutorialCanvas.SetActive(true);
    }

    /// <summary>
    /// The CloseTutorialPressed function plays a click sound, actiavates the menu canvas, and deactivates the
    /// tutorial Canvas.
    /// </summary>
    public void CloseTutorialPressed()
    {
        audioSource.PlayOneShot(clickSound);
        menuCanvas.SetActive(true);
        tutorialCanvas.SetActive(false);
    }

    /// <summary>
    /// The function "PlayerSelected" is used to handle the selection of a player in a menu, updating
    /// the UI and saving the selected player's data.
    /// </summary>
    /// <param name="ID">The ID parameter is an integer that represents the selected player's ID. It is
    /// used to identify the player in the game.</param>
    /// <param name="MenuPlayerID">MenuPlayerID is an enum that represents the different player images
    /// available in the menu.</param>
    public void PlayerSelected(int ID, MenuPlayerID image)
    {
        audioSource.PlayOneShot(clickSound);
        if (previousSelectedPlayerImage != null)
            previousSelectedPlayerImage.color = defaultColour;
        currentSelectedPlayerImage = image.gameObject.GetComponent<Image>();
        previousSelectedPlayerImage = currentSelectedPlayerImage;
        currentSelectedPlayerImage.color = selectedColour;
        currentSelectedPlayer = ID;
        var pickedData = SaveManager.instance.LoadGeneralSaveData();
        pickedData.playerID = currentSelectedPlayer;
        SaveManager.instance.SaveGeneralData(pickedData);
        PhotonNetwork.NickName = SaveManager.instance.LoadPlayerSaveData().players[ID].playerName;
    }
    /// <summary>
    /// The function MapSelected takes an ID and an image, plays a click sound, changes the color of the
    /// selected map image, updates the current selected map ID, and saves the general data with the
    /// updated map ID.
    /// </summary>
    /// <param name="ID">The ID parameter is an integer that represents the selected map's ID. It is
    /// used to identify the specific map that was selected.</param>
    /// <param name="MenuMapID">MenuMapID is an enum type that represents the different map images in
    /// the menu.</param>
    public void MapSelected(int ID, MenuMapID image)
    {
        audioSource.PlayOneShot(clickSound);
        if (previousSelectedMapImage != null)
            previousSelectedMapImage.color = defaultColour;
        currentSelectedMapImage = image.gameObject.GetComponent<Image>();
        previousSelectedMapImage = currentSelectedMapImage;
        currentSelectedMapImage.color = selectedColour;
        currentSelectedMap = ID;
        var pickedData = SaveManager.instance.LoadGeneralSaveData();
        pickedData.mapID = currentSelectedMap;
        SaveManager.instance.SaveGeneralData(pickedData);
    }

    /// <summary>
    /// The DeletePlayer function removes a player from the list of saved players and updates the UI.
    /// </summary>
    /// <returns>
    /// If the condition `if (currentSelectedPlayer == -1)` is true, then nothing is being returned and
    /// the method will exit. If the condition is false, then the method will continue executing the
    /// remaining code without returning anything.
    /// </returns>
    public void DeletePlayer()
    {
        audioSource.PlayOneShot(clickSound);
        if (currentSelectedPlayer == -1)
            return;
        var currentPlayers = SaveManager.instance.LoadPlayerSaveData();
        currentPlayers.players.RemoveAt(currentSelectedPlayer);
        SaveManager.instance.SavePlayerData(currentPlayers);
        LoadSavedPlayers();

        currentSelectedPlayer = -1;
    }
    /// <summary>
    /// The DeleteMap function deletes a selected map from a list of saved maps and updates the UI.
    /// </summary>
    /// <returns>
    /// If the condition `if (currentSelectedMap == -1)` is true, then nothing is being returned and the
    /// method will exit. If the condition is false, then the method will continue executing the code
    /// below and nothing will be explicitly returned.
    /// </returns>
    public void DeleteMap()
    {
        audioSource.PlayOneShot(clickSound);
        if (currentSelectedMap == -1)
            return;
        var currentMaps = SaveManager.instance.LoadMapSaveData();
        currentMaps.maps.RemoveAt(currentSelectedMap);
        SaveManager.instance.SaveMapData(currentMaps);
        LoadSavedMaps();

        currentSelectedMap = -1;
    }

    /// <summary>
    /// The CancelPressed function plays a click sound and deactivates the playerCreationUI and
    /// mapCreationUI game objects.
    /// </summary>
    public void CancelPressed()
    {
        audioSource.PlayOneShot(clickSound);
        playerCreationUI.SetActive(false);
        mapCreationUI.SetActive(false);
    }

    /// <summary>
    /// The CreateMapPressed function adds a new map to the current maps list, saves the updated map
    /// data, and hides the map creation UI.
    /// </summary>
    /// <returns>
    /// If the length of the text in the mapNameEntry is less than or equal to 3, then nothing is being
    /// returned. The return statement is used to exit the method early and prevent the rest of the code
    /// from executing.
    /// </returns>
    public void CreateMapPressed()
    {
        audioSource.PlayOneShot(clickSound);
        if (mapNameEntry.text.Length <= 3)
            return;
        var currentMaps = SaveManager.instance.LoadMapSaveData();
        MapData newMap = new MapData(mapNameEntry.text);
        currentMaps.maps.Add(newMap);
        SaveManager.instance.SaveMapData(currentMaps);
        LoadSavedMaps();
        mapCreationUI.SetActive(false);
    }
    
    /// <summary>
    /// The function creates a new player, adds basic tools to their inventory and hotbar, saves the
    /// player data, and hides the player creation UI.
    /// </summary>
    /// <returns>
    /// If the length of the player name entry is less than or equal to 3, the function will return and
    /// not execute the rest of the code.
    /// </returns>
    public void CreatePlayerPressed()
    {
        audioSource.PlayOneShot(clickSound);
        if (playerNameEntry.text.Length <= 3)
            return;
        var currentPlayers = SaveManager.instance.LoadPlayerSaveData();
        PlayerData newPlayer = new PlayerData(playerNameEntry.text);
        // add the basic tools to the player inventory
        currentPlayers.players.Add(newPlayer);
        currentPlayers.players[currentPlayers.players.Count - 1].inventory.Add(5, 1);
        currentPlayers.players[currentPlayers.players.Count - 1].inventory.Add(6, 1);
        currentPlayers.players[currentPlayers.players.Count - 1].inventory.Add(7, 1);
        currentPlayers.players[currentPlayers.players.Count - 1].hotbar.Add(5, true);
        currentPlayers.players[currentPlayers.players.Count - 1].hotbar.Add(6, true);
        currentPlayers.players[currentPlayers.players.Count - 1].hotbar.Add(7, true);
        SaveManager.instance.SavePlayerData(currentPlayers);
        LoadSavedPlayers();
        playerCreationUI.SetActive(false);
    }

    /// <summary>
    /// The function "CreateNewPlayerPressed" plays a click sound and activates the player creation UI.
    /// </summary>
    public void CreateNewPlayerPressed()
    {
        audioSource.PlayOneShot(clickSound);
        playerCreationUI.SetActive(true);
    }
    /// <summary>
    /// The function "CreateNewMapPressed" plays a click sound and activates the mapCreationUI object.
    /// </summary>
    public void CreateNewMapPressed()
    {
        audioSource.PlayOneShot(clickSound);
        mapCreationUI.SetActive(true);
    }

    /// <summary>
    /// The function "LoadSavedPlayers" loads saved player data and creates menu player objects with
    /// corresponding information.
    /// </summary>
    public void LoadSavedPlayers()
    {
        for (int i = 0; i < loadedPlayers.Count; i++)
        {
            Destroy(loadedPlayers[i]);
        }
        loadedPlayers.Clear();

        var savedPlayers = SaveManager.instance.LoadPlayerSaveData();
        for (int i = 0; i < savedPlayers.players.Count; i++)
        {
            // add an item for each of the players
            var tempPlayer = Instantiate(menuPlayerObject, playerIconSpawnPosition).GetComponent<MenuPlayerID>();
            tempPlayer.ID = i;
            tempPlayer.playerName.text = savedPlayers.players[i].playerName;
            for (int j = 0; j < tempPlayer.heartIcons.Length; j++)
            {
                if (savedPlayers.players[i].hearts > j)
                    tempPlayer.heartIcons[j].sprite = heartIcon;
                else
                    tempPlayer.heartIcons[j].sprite = emptyHeartIcon;
            }
            tempPlayer.Setup(this);
            loadedPlayers.Add(tempPlayer.gameObject);
        }
    }
    /// <summary>
    /// The function "LoadSavedMaps" loads saved map data and creates menu map objects based on the
    /// saved data.
    /// </summary>
    public void LoadSavedMaps()
    {
        currentSelectedMap = -1;
        currentSelectedPlayer = -1;
        previousSelectedMapImage = null;
        previousSelectedPlayerImage = null;
        for (int i = 0; i < loadedMaps.Count; i++)
        {
            Destroy(loadedMaps[i]);
        }
        loadedMaps.Clear();

        var savedMaps = SaveManager.instance.LoadMapSaveData();
        for (int i = 0; i < savedMaps.maps.Count; i++)
        {
            // add an item for each of the maps
            var tempMap = Instantiate(menuMapObject, mapIconSpawnPosition).GetComponent<MenuMapID>();
            tempMap.ID = i;
            tempMap.mapName.text = savedMaps.maps[i].mapName;
            if (savedMaps.maps[i].section4Unlocked)
                tempMap.mapIcon.sprite = mapIcons[4];
            else if (savedMaps.maps[i].section3Unlocked)
                tempMap.mapIcon.sprite = mapIcons[3];
            else if (savedMaps.maps[i].section2Unlocked)
                tempMap.mapIcon.sprite = mapIcons[2];
            else if (savedMaps.maps[i].section1Unlocked)
                tempMap.mapIcon.sprite = mapIcons[1];
            else
                tempMap.mapIcon.sprite = mapIcons[0];
            tempMap.Setup(this);
            loadedMaps.Add(tempMap.gameObject);
        }
    }

    /// <summary>
    /// The Start function joins the Photon lobby, initializes variables for the selected map and
    /// player, and loads saved player and map data.
    /// </summary>
    public void Start()
    {
        PhotonNetwork.JoinLobby();
        cachedRoomList.Clear();
        currentSelectedMap = -1;
        currentSelectedPlayer = -1;
        LoadSavedPlayers();
        LoadSavedMaps();
    }

    /// <summary>
    /// The Update function checks if the current client is the master client in a Photon Network game
    /// and activates or deactivates a start button accordingly.
    /// </summary>
    public void Update()
    {
        if (PhotonNetwork.IsMasterClient)
            startButton.SetActive(true);
        else
            startButton.SetActive(false);
    }

    /// <summary>
    /// The StartClicked function plays a click sound and loads the "SampleScene" level in
    /// PhotonNetwork. as well as making the current room invisable to other players.
    /// </summary>
    public void StartClicked()
    {
        audioSource.PlayOneShot(clickSound);
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel("SampleScene");
    }

    /// <summary>
    /// The function "CreateClicked" plays a click sound and creates a room in Photon Network if the
    /// room name, selected map, and selected player are valid.
    /// </summary>
    public void CreateClicked()
    {
        audioSource.PlayOneShot(clickSound);
        if (roomInput.text.Length >= 1 && currentSelectedMap != -1 && currentSelectedPlayer != -1)
        {
            PhotonNetwork.CreateRoom(roomInput.text, new Photon.Realtime.RoomOptions() { MaxPlayers = 4 , BroadcastPropsChangeToAll = true});
        }
    }

    /// <summary>
    /// The function "OnJoinedRoom" is called when the player successfully joins a room, and it updates
    /// the UI to display the room name and player list.
    /// </summary>
    public override void OnJoinedRoom()
    {
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);
        roomName.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name;
        UpdatePlayerList();
    }

    /// <summary>
    /// The function "OnRoomListUpdate" checks if enough time has passed since the last update and then
    /// updates the room list.
    /// </summary>
    /// <param name="list">The "list" parameter is a List of RoomInfo objects. It represents the updated
    /// list of rooms available in the network.</param>
    public override void OnRoomListUpdate(List<RoomInfo> list)
    {
        if (Time.time > roomListUpdateDelay + _timeOfLastUpdate)
        {
            _timeOfLastUpdate = Time.time;
            UpdateRoomList(list);
        }
    }

    /// <summary>
    /// The function updates the list of rooms by destroying the current room items and creating new ones
    /// based on the provided list of room information.
    /// </summary>
    /// <param name="list">A list of RoomInfo objects that contains information about each room.</param>
    public void UpdateRoomList(List<RoomInfo> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            RoomInfo info = list[i];
            if (info.RemovedFromList)
            {
                Destroy(cachedRoomList[info.Name].gameObject);
                cachedRoomList.Remove(info.Name);
            }
            else
            {
                if (!cachedRoomList.ContainsKey(info.Name))
                {
                    RoomItem tempRoom = Instantiate(roomItem, spawnArea).GetComponent<RoomItem>();
                    tempRoom.SetRoomName(info.Name);
                    cachedRoomList[info.Name] = tempRoom;
                }
            }
        }
    }

    /// <summary>
    /// The function updates the player list by destroying existing player items, clearing the list, and
    /// then creating new player items based on the current players in the PhotonNetwork room.
    /// </summary>
    /// <returns>
    /// If the condition `if (PhotonNetwork.CurrentRoom == null)` is true, then the method will return and
    /// no further code will be executed. If the condition is false, then the method will continue executing
    /// the code inside the foreach loop.
    /// </returns>
    public void UpdatePlayerList()
    {
        foreach (PlayerItem item in currentPlayers)
        {
            Destroy(item.gameObject);
        }
        currentPlayers.Clear();

        if (PhotonNetwork.CurrentRoom == null)
            return;

        foreach (KeyValuePair<int, Photon.Realtime.Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            PlayerItem tempPlayer = Instantiate(playerPrefab, playerSpawnArea).GetComponent<PlayerItem>();
            tempPlayer.SetPlayerName(player.Value);

            if (player.Value == PhotonNetwork.LocalPlayer)
                tempPlayer.ApplyLocalChanges();
            currentPlayers.Add(tempPlayer);
        }
    }


    /// <summary>
    /// The JoinRoom function checks if a map and player have been selected, and if so, it joins the
    /// specified room.
    /// </summary>
    /// <param name="roomName">The room name is a string that represents the name of the room that the
    /// player wants to join.</param>
    public void JoinRoom(string roomName)
    {
        if (currentSelectedMap != -1 && currentSelectedPlayer != -1)
            PhotonNetwork.JoinRoom(roomName);
    }

    /// <summary>
    /// The function "LeaveClicked" plays a click sound and then leaves the current Photon room.
    /// </summary>
    public void LeaveClicked()
    {
        audioSource.PlayOneShot(clickSound);
        PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// The function "OnConnectedToMaster" is called when the player successfully connects to the Photon
    /// server, and it then joins the lobby.
    /// </summary>
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        cachedRoomList.Clear();
    }

    /// <summary>
    /// The function OnLeftRoom() hides the roomPanel and shows the lobbyPanel.
    /// </summary>
    public override void OnLeftRoom()
    {
        roomPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    /// <summary>
    /// The function "OnPlayerEnteredRoom" is called when a new player enters the room and it updates
    /// the player list.
    /// </summary>
    /// <param name="newPlayer">The new player who entered the room.</param>
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        UpdatePlayerList();
    }

    /// <summary>
    /// The function "OnPlayerLeftRoom" is called when a player leaves the room and it updates
    /// the player list.
    /// </summary>
    /// <param name="newPlayer">The new player who left the room.</param>
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        UpdatePlayerList();
    }
}
