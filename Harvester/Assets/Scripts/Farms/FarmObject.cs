using JetBrains.Annotations;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
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
    private string _interactedPlayer;

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

    [Header("UI")]
    public GameObject interactionUI;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip collected;

    void Start()
    {
        _timeOfLastClose = Time.time;

        inventory = GameObject.FindGameObjectWithTag("Manager").GetComponent<Inventory>();
        interactionUI = GameObject.FindGameObjectWithTag("Manager").GetComponent<MiscManager>().interactUI;

        // farm name
        stationName.text = farmData.Farms[farmID].farmName;

        closeMenuButton.onClick.AddListener(() => CloseMenu());
        collectButton.onClick.AddListener(() => CollectPressed());
    }

    public void CollectPressed()
    {
        audioSource.PlayOneShot(collected);

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
        UpdateUI();
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
        if (Input.GetKeyDown(KeyCode.E) && inRange)
        {
            if (MenuManager.IsCurrentMenuClose(MenuID.FARM))
            {
                // Close
                CloseMenu();
            }
            else if (MenuManager.CanOpenMenuSet(MenuID.FARM))
            {
                // Open
                OpenMenu();
            }
        }

        // only update all of the values if the owner of the object (this should mean it is only done once)
        PhotonView view = PhotonView.Get(gameObject);
        if (!view.IsMine)
            return;
        if (Time.time > _timeOfLastClose + generationTime)
        {
            // increase the amount of one of the items held
            _timeOfLastClose = Time.time;
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("AddItem", RpcTarget.All);
            photonView.RPC("SetCount", RpcTarget.All, count);
        }
    }

    /*
    public void calculateNumbers()
    {
        PhotonView photonView = PhotonView.Get(this);
        var currentTime = Time.time;
        int totalItemCount = (int)((currentTime - _timeOfLastClose) / generationTime);
        for (int i = 0; i < totalItemCount; i++)
        {
            photonView.RPC("AddItem", RpcTarget.MasterClient);
        }
        UpdateUI();
        photonView.RPC("SetCount", RpcTarget.All, count);
        _timeOfLastClose = Time.time;
    }
    */

    [PunRPC]
    public void AddItem()
    {
        PhotonView view = PhotonView.Get(gameObject);
        if (!view.IsMine)
            return;
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
        if (collision.CompareTag("Player") && !inRange)
        {
            inRange = true;
            _interactedPlayer = collision.name;
            interactionUI.SetActive(true);
        }
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && _interactedPlayer.Equals(collision.name))
        {
            inRange = false;
            _interactedPlayer = null;
            interactionUI.SetActive(false);
            UI.SetActive(false);
            MenuManager.menuOpen = MenuID.NOTHING;
        }
    }

    public void OpenMenu()
    {
        //calculateNumbers();
        UI.SetActive(true);
    }
    public void CloseMenu()
    {
        MenuManager.menuOpen = MenuID.NOTHING;
        UI.SetActive(false);
    }
}
