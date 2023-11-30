using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Data.Odbc;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField roomInput;
    public GameObject lobbyPanel;
    public GameObject roomPanel;
    public TMP_Text roomName;

    [Header("Rooms")]
    public GameObject roomItem;
    public List<RoomItem> currentRooms = new();
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

    public void PlayerSelected(int ID, MenuPlayerID image)
    {
        if (previousSelectedPlayerImage != null)
            previousSelectedPlayerImage.color = defaultColour;
        currentSelectedPlayerImage = image.gameObject.GetComponent<Image>();
        previousSelectedPlayerImage = currentSelectedPlayerImage;
        currentSelectedPlayerImage.color = selectedColour;
        currentSelectedPlayer = ID;
        PhotonNetwork.NickName = SaveManager.instance.LoadPlayerSaveData().players[ID].playerName;
    }
    public void MapSelected(int ID, MenuMapID image)
    {
        if (previousSelectedMapImage != null)
            previousSelectedMapImage.color = defaultColour;
        currentSelectedMapImage = image.gameObject.GetComponent<Image>();
        previousSelectedMapImage = currentSelectedMapImage;
        currentSelectedMapImage.color = selectedColour;
        currentSelectedMap = ID;
    }

    public void DeletePlayer()
    {
        if (currentSelectedPlayer == -1)
            return;
        var currentPlayers = SaveManager.instance.LoadPlayerSaveData();
        currentPlayers.players.RemoveAt(currentSelectedPlayer);
        SaveManager.instance.SavePlayerData(currentPlayers);
        LoadSavedPlayers();
    }
    public void DeleteMap()
    {
        if (currentSelectedMap == -1)
            return;
        var currentMaps = SaveManager.instance.LoadMapSaveData();
        currentMaps.maps.RemoveAt(currentSelectedMap);
        SaveManager.instance.SaveMapData(currentMaps);
        LoadSavedMaps();
    }

    public void CancelPressed()
    {
        playerCreationUI.SetActive(false);
        mapCreationUI.SetActive(false);
    }

    public void CreateMapPressed()
    {
        if (mapNameEntry.text.Length <= 3)
            return;
        var currentMaps = SaveManager.instance.LoadMapSaveData();
        MapData newMap = new MapData(mapNameEntry.text);
        currentMaps.maps.Add(newMap);
        SaveManager.instance.SaveMapData(currentMaps);
        LoadSavedMaps();
        mapCreationUI.SetActive(false);
    }
    public void CreatePlayerPressed()
    {
        if (playerNameEntry.text.Length <= 3)
            return;
        var currentPlayers = SaveManager.instance.LoadPlayerSaveData();
        PlayerData newPlayer = new PlayerData(playerNameEntry.text);
        currentPlayers.players.Add(newPlayer);
        SaveManager.instance.SavePlayerData(currentPlayers);
        LoadSavedPlayers();
        playerCreationUI.SetActive(false);
    }

    public void CreateNewPlayerPressed()
    {
        playerCreationUI.SetActive(true);
    }
    public void CreateNewMapPressed()
    {
        mapCreationUI.SetActive(true);
    }

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

    public void Start()
    {
        PhotonNetwork.JoinLobby();
        currentSelectedMap = -1;
        currentSelectedPlayer = -1;
        LoadSavedPlayers();
        LoadSavedMaps();
    }

    public void Update()
    {
        if (PhotonNetwork.IsMasterClient)
            startButton.SetActive(true);
        else
            startButton.SetActive(false);
    }

    public void StartClicked()
    {
        PhotonNetwork.LoadLevel("SampleScene");
    }

    public void CreateClicked()
    {
        if(roomInput.text.Length >= 1 && currentSelectedMap != -1 && currentSelectedPlayer != -1)
        {
            PhotonNetwork.CreateRoom(roomInput.text, new Photon.Realtime.RoomOptions() { MaxPlayers = 3 , BroadcastPropsChangeToAll = true});
        }
    }

    public override void OnJoinedRoom()
    {
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);
        roomName.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name;
        UpdatePlayerList();
    }

    public override void OnRoomListUpdate(List<RoomInfo> list)
    {
        if (Time.time > roomListUpdateDelay + _timeOfLastUpdate)
        {
            _timeOfLastUpdate = Time.time;
            UpdateRoomList(list); 
        }
    }

    public void UpdateRoomList(List<RoomInfo> list)
    {
        foreach (RoomItem item in currentRooms)
        {
            Destroy(item.gameObject);
        }
        currentRooms.Clear();

        foreach (RoomInfo room in list)
        {
            RoomItem tempRoom = Instantiate(roomItem, spawnArea).GetComponent<RoomItem>();
            tempRoom.SetRoomName(room.Name);
            currentRooms.Add(tempRoom);
        }
    }
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


    public void JoinRoom(string roomName)
    {
        if (currentSelectedMap != -1 && currentSelectedPlayer != -1)
            PhotonNetwork.JoinRoom(roomName);
    }

    public void LeaveClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnLeftRoom()
    {
        roomPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        UpdatePlayerList();
    }

}
