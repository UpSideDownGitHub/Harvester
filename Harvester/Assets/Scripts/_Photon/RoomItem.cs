using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomItem : MonoBehaviour
{
    public TMP_Text roomName;
    public LobbyManager manager;

    public void Start()
    {
        manager = FindObjectOfType<LobbyManager>();
    }

    public void SetRoomName(string name)
    {
        roomName.text = name;
    }

    public void ButtonClicked()
    {
        manager.JoinRoom(roomName.text);
    }
}
