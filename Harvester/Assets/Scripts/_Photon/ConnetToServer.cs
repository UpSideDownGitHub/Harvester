using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConnetToServer : MonoBehaviourPunCallbacks
{
    public void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        SceneManager.LoadScene("Lobby");
    }
}

//public TMP_InputField usernameInput;
//public TMP_Text buttonText;
//public void ConnectClicked()
//{
//    if (usernameInput.text.Length >= 1)
//    {
//        PhotonNetwork.NickName = usernameInput.text;
//        buttonText.text = "Connecting...";
//        PhotonNetwork.AutomaticallySyncScene = true;
//        PhotonNetwork.ConnectUsingSettings();
//    }
//}
