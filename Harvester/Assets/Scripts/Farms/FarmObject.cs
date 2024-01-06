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
    public AudioClip closeMenu;
    public AudioClip openMenu;

    /// <summary>
    /// Initialization method called when the object is started.
    /// </summary>
    /// <remarks>
    /// This method initializes variables and UI elements, such as setting the farm name, 
    /// configuring button listeners, and retrieving references to the inventory and interaction UI components.
    /// </remarks>
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

    /// <summary>
    /// Handles the button press event for collecting items.
    /// </summary>
    /// <remarks>
    /// This method plays the collected sound effect, adds items to the inventory based on the farm's configuration, 
    /// updates the UI, and synchronizes the item count across the network using a Photon RPC.
    /// </remarks>
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

    /// <summary>
    /// RPC method to set the item count and update the UI.
    /// </summary>
    /// <param name="value">The new item count values to set.</param>
    [PunRPC]
    public void SetCount(int[] value)
    {
        count = value;
        UpdateUI();
    }

    /// <summary>
    /// Updates the UI elements to display the current item counts.
    /// </summary>
    public void UpdateUI()
    {
        for (int i = 0; i < count.Length; i++)
        {
            itemCounts[i].text = "x" + count[i];
        }
    }

    /// <summary>
    /// Update method called each frame to handle player input and update object values.
    /// </summary>
    /// <remarks>
    /// This method checks for the "E" key input and whether the player is in range.
    /// If the conditions are met, it toggles between opening and closing the farm menu based on its current state.
    /// Additionally, it only updates values for the owner of the object, ensuring it is done only once per frame.
    /// The method also triggers the periodic increase of item counts and synchronization across the network.
    /// </remarks>
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && inRange)
        {
            if (MenuManager.IsCurrentMenuClose(MenuID.FARM))
            {
                // Close
                audioSource.PlayOneShot(closeMenu);
                CloseMenu();
            }
            else if (MenuManager.CanOpenMenuSet(MenuID.FARM))
            {
                // Open
                audioSource.PlayOneShot(openMenu);
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

    /// <summary>
    /// RPC method to add a randomly selected item to the inventory.
    /// </summary>
    /// <remarks>
    /// This method is invoked remotely to add a randomly selected item to the inventory based on predefined chances.
    /// </remarks>
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

    /// <summary>
    /// Triggered when another Collider2D enters this object's trigger zone.
    /// </summary>
    /// <param name="collision">The Collider2D entering the trigger zone.</param>
    /// <remarks>
    /// This method checks if the entering collider has the "Player" tag and if the object is not already in range.
    /// If the conditions are met, it sets the object as in range, records the interacting player's name, 
    /// and activates the interaction UI.
    /// </remarks>
    public void OnTriggerEnter2D(Collider2D collision)
    {
        PhotonView view = PhotonView.Get(collision);
        if (view.IsMine)
        {
            if (collision.CompareTag("Player") && !inRange)
            {

                inRange = true;
                _interactedPlayer = collision.name;
                interactionUI.SetActive(true);
            }
        }
    }
    /// <summary>
    /// Triggered when another Collider2D exits this object's trigger zone.
    /// </summary>
    /// <param name="collision">The Collider2D exiting the trigger zone.</param>
    /// <remarks>
    /// This method checks if the exiting collider has the "Player" tag and if it matches the previously recorded interacting player.
    /// If the conditions are met, it sets the object as out of range, clears the recorded interacting player, 
    /// deactivates the interaction UI and associated UI elements, and updates the global menu state.
    /// </remarks>
    public void OnTriggerExit2D(Collider2D collision)
    {
        PhotonView view = PhotonView.Get(collision);
        if (view.IsMine)
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
    }

    /// <summary>
    /// Opens the farm menu.
    /// </summary>
    public void OpenMenu()
    {
        //calculateNumbers();
        UI.SetActive(true);
    }
    /// <summary>
    /// Closes the farm menu.
    /// </summary>
    public void CloseMenu()
    {
        MenuManager.menuOpen = MenuID.NOTHING;
        UI.SetActive(false);
    }
}
