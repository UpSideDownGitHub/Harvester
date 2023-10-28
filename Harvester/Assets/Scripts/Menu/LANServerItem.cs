using FishNet;
using FishNet.Discovery;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LANServerItem : MonoBehaviour
{
    public string ipAddress;
    public TMP_Text ipText;
    public TMP_Text port;
    public int clients;
    public TMP_Text players;
    public Button joinButton;
    public int maxPlayers;

    NetworkDiscovery discovery;

    public void SetInfo(string ip, string port, int players, NetworkDiscovery discovery, int maxPlayers)
    {
        this.ipAddress = ip;
        this.ipText.text = ip;
        this.port.text = port;
        this.clients = players;
        this.players.text = players.ToString();
        this.discovery = discovery;
        this.maxPlayers = maxPlayers;
        joinButton.onClick.AddListener(() => JoinServer());
    }

    public void JoinServer()
    {
        if (clients >= maxPlayers)
            return;
        discovery.StopSearchingOrAdvertising();
        InstanceFinder.ClientManager.StartConnection(ipAddress);
    }
}
