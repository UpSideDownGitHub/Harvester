using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


/* This class connects to a server using PhotonNetwork and loads the "Lobby" scene when connected to
the master server. */
public class ConnetToServer : MonoBehaviourPunCallbacks
{
    /// <summary>
    /// The Start function in C# sets up PhotonNetwork to automatically sync scenes and connects to the
    /// network using the current settings.
    /// </summary>
    public void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    /// <summary>
    /// The function "OnConnectedToMaster" loads the "Lobby" scene when the player is connected to the
    /// master server.
    /// </summary>
    public override void OnConnectedToMaster()
    {
        SceneManager.LoadScene("Lobby");
    }
}