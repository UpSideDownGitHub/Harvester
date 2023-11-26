using JetBrains.Annotations;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class FarmObject : MonoBehaviour
{
    [Header("Inventory")]
    public Inventory inventory;

    [Header("World Object")]
    public GameObject UI;
    public Button closeMenuButton;
    public Button collectButton;
    public bool inRange;

    [Header("Station ID")]
    public FarmData farmData;
    public int farmID;

    [Header("UI")]
    public TMP_Text stationName;
    public TMP_Text[] itemCounts;

    [Header("Generation")]
    public float generationTime;
    public int[] count;
    private float _timeOfLastClose;

    [Header("Dropping Items")]
    public GameObject pickupItem;


    void Start()
    {
        _timeOfLastClose = Time.time;

        inventory = GameObject.FindGameObjectWithTag("Manager").GetComponent<Inventory>();

        // farm name
        stationName.text = farmData.Farms[farmID].farmName;

        closeMenuButton.onClick.AddListener(() => CloseMenu());
        collectButton.onClick.AddListener(() => CollectPressed());

    }

    public void CollectPressed()
    {
        // collect the items
        for (int i = 0; i < count.Length; i++)
        {
            inventory.AddItem(farmData.Farms[farmID].items[i].item, count[i]);
            count[i] = 0;
        }
        UpdateUI();
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("SetCount", RpcTarget.All, count);

    }

    [PunRPC]
    public void SetCount(int[] value)
    {
        count = value;
    }

    public void UpdateUI()
    {
        for (int i = 0; i < count.Length; i++)
        {
            itemCounts[i].text = "x" + count[i];
        }
    }

    public void Update()
    {
        if (UI.activeInHierarchy)
        {
            // update the numbers real time other wise just do it when collected
            if (Time.time > _timeOfLastClose + generationTime)
            {
                // increase the amount of one of the items held
                _timeOfLastClose = Time.time;
                AddItem();
                UpdateUI();
                PhotonView photonView = PhotonView.Get(this);
                photonView.RPC("SetCount", RpcTarget.All, count);
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && inRange)
        {
            if (UI.activeInHierarchy)
                CloseMenu();
            else
                OpenMenu();
        }
    }

    public void calculateNumbers()
    {
        var currentTime = Time.time;
        int totalItemCount = (int)((currentTime - _timeOfLastClose) / generationTime);
        for (int i = 0; i < totalItemCount; i++)
        {
            AddItem();
        }
        UpdateUI();
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("SetCount", RpcTarget.All, count);
        _timeOfLastClose = Time.time;
    }
    public void AddItem()
    {
        float rand = Random.value;
        for (int j = 0; j < farmData.Farms[farmID].items.Length; j++)
        {
            float previous = 0;
            if (j != 0)
                previous = farmData.Farms[farmID].items[j - 1].chance;

            if (previous < rand && rand <= farmData.Farms[farmID].items[j].chance)
            {
                count[j] += 1;
                break;
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            inRange = true;
        }
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            inRange = false;
            UI.SetActive(false);
        }
    }

    public void OpenMenu()
    {
        calculateNumbers();
        UI.SetActive(true);
    }
    public void CloseMenu()
    {
        UI.SetActive(false);
    }
}
