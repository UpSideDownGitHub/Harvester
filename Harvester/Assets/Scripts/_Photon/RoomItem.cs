using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/* The RoomItem class is a MonoBehaviour that represents an item in a room list and allows the user to
join a room when a button is clicked. */
public class RoomItem : MonoBehaviour
{
    public TMP_Text roomName;
    public LobbyManager manager;

    /// <summary>
    /// The Start function finds and assigns the LobbyManager component in the scene.
    /// </summary>
    public void Start()
    {
        manager = FindObjectOfType<LobbyManager>();
    }

    /// <summary>
    /// The SetRoomName function sets the text of a roomName variable to the specified name.
    /// </summary>
    /// <param name="name">The name of the room that you want to set.</param>
    public void SetRoomName(string name)
    {
        roomName.text = name;
    }

    /// <summary>
    /// The ButtonClicked function calls the JoinRoom method of the manager object, passing in the value
    /// of the roomName text field.
    /// </summary>
    public void ButtonClicked()
    {
        manager.JoinRoom(roomName.text);
    }
}
