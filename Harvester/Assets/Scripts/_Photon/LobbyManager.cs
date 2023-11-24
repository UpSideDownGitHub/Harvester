using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Data.Odbc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

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

    public void Start()
    {
        PhotonNetwork.JoinLobby();
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
        if(roomInput.text.Length >= 1)
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
