using FishNet;
using FishNet.Discovery;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class LANServerManager : MonoBehaviour
{
    [SerializeField]
    private NetworkDiscovery networkDiscovery;
    public int maxPlayers = 4;

    private readonly List<IPEndPoint> _endPoints = new List<IPEndPoint>();

    public Transform spawnPosition;
    public GameObject ServerItemObject;
    public List<GameObject> SpawnedServerObjects = new();
    public List<string> SpawnedServerIPS = new();
    public List<bool> checkedServer = new();



    public bool runningServer;

    void Start()
    {
        if (networkDiscovery == null) networkDiscovery = FindObjectOfType<NetworkDiscovery>();

        networkDiscovery.ServerFoundCallback += endPoint =>
        {
            if (!_endPoints.Contains(endPoint)) _endPoints.Add(endPoint);
        };
    }

    public void ToggleServer()
    {
        runningServer = !runningServer;
        if (runningServer)
            InstanceFinder.ServerManager.StartConnection();
        else
            InstanceFinder.ServerManager.StopConnection(true);
    }
    public void ToggleAdvertisingServer()
    {
        if (networkDiscovery.IsAdvertising)
            networkDiscovery.StopSearchingOrAdvertising();
        else
            networkDiscovery.AdvertiseServer();
    }
    public void ToggleSearching()
    {
        if (networkDiscovery.IsSearching)
            networkDiscovery.StopSearchingOrAdvertising();
        else
            networkDiscovery.SearchForServers();
    }

    void Update()
    {
        if (_endPoints.Count < 1)
        {
            return;
        }
        for (int i = 0; i < checkedServer.Count; i++)
        {
            checkedServer[i] = false;
        }

        foreach (IPEndPoint endPoint in _endPoints)
        {
            string ipAddress = endPoint.Address.ToString();
            if (SpawnedServerIPS.Contains(ipAddress))
            {
                checkedServer[SpawnedServerIPS.IndexOf(ipAddress)] = true;
                continue;
            }

            string port = endPoint.Port.ToString();
            int clients = InstanceFinder.ClientManager.Clients.Count;

            var serverItem = Instantiate(ServerItemObject, spawnPosition).GetComponent<LANServerItem>();
            SpawnedServerObjects.Add(serverItem.gameObject);
            SpawnedServerIPS.Add(ipAddress);
            checkedServer.Add(true);
            serverItem.SetInfo(ipAddress, port, clients, networkDiscovery, maxPlayers);
            
            /*
            if (!GUILayout.Button(ipAddress)) continue;
            networkDiscovery.StopSearchingOrAdvertising();
            InstanceFinder.ClientManager.StartConnection(ipAddress);
            */
        }

        for (int i = 0; i < checkedServer.Count; i++)
        {
            if (!checkedServer[i])
            {
                Destroy(SpawnedServerObjects[i]);
                SpawnedServerObjects.RemoveAt(i);
                SpawnedServerIPS.RemoveAt(i);
                checkedServer.RemoveAt(i);
            }
        }
    }
}
