using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Policy;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Dictionary<Item, int> inventory = new();
    public Dictionary<Item, bool> hotbar = new();

    [Header("UI")]
    public GameObject inventoryCanvas;
    public GameObject craftingCanvas;
    public List<GameObject> currentItems = new();
    public Transform spawnLocation;
    public GameObject itemPrefab;

    [Header("Data")]
    public ItemData data;

    [Header("Hotbar")]
    public Transform hotBarSpawnLocation;
    public GameObject hotbarItemPrefab;
    public List<GameObject> currentHotbarItems = new();
    public Player player;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip open;
    public AudioClip close;
    public AudioClip click;
    public AudioClip pin;

/// <summary>
/// Checks if the inventory canvas is currently open.
/// </summary>
/// <returns>True if the inventory canvas is active, false otherwise.</returns>
/// <remarks>
/// This method returns a boolean indicating whether the inventory canvas is currently open or not.
/// </remarks>
    public bool isInventoryOpen()
    {
        return inventoryCanvas.activeInHierarchy;
    } 

/// <summary>
/// Saves the player's inventory and hotbar data to the file.
/// </summary>
/// <remarks>
/// This method retrieves the current player and save data, clears existing inventory and hotbar data, 
/// and populates them with the current inventory and hotbar items. It then saves the data using the SaveManager.
/// </remarks>
    public void SavePlayerData()
    {
        // Save the current map data to the file
        var pickedData = SaveManager.instance.LoadGeneralSaveData();
        var saveData = SaveManager.instance.LoadPlayerSaveData();
        var playerData = saveData.players[pickedData.playerID];
        playerData.inventory.Clear();
        playerData.hotbar.Clear();
        foreach (KeyValuePair<Item, int> item in inventory)
        {
            playerData.inventory.Add(item.Key.itemID, item.Value);
        }
        foreach (KeyValuePair<Item, bool> item in hotbar)
        {
            playerData.hotbar.Add(item.Key.itemID, item.Value);
        }
        saveData.players[pickedData.playerID] = playerData;
        saveData.players[pickedData.playerID].hearts = player.curHealth;
        SaveManager.instance.SavePlayerData(saveData);
    }

/// <summary>
/// Sets the inventory and hotbar with the provided data.
/// </summary>
/// <param name="newInventory">The new inventory data.</param>
/// <param name="newHotbar">The new hotbar data.</param>
/// <remarks>
/// This method clears the existing inventory and hotbar, then adds items from the provided data.
/// It updates the UI accordingly if the inventory canvas is active.
/// </remarks>
    public void SetInventory(Dictionary<int, int> newInventory, Dictionary<int, bool> newHotbar)
    {
        inventory.Clear();
        hotbar.Clear();
        foreach (KeyValuePair<int, int> item in newInventory)
        {
            inventory.Add(data.items[item.Key], item.Value);
        }
        foreach (KeyValuePair<int, bool> item in newHotbar)
        {
            hotbar.Add(data.items[item.Key], item.Value);
        }


        if (inventoryCanvas.activeInHierarchy)
            UpdateUI();
        UpdateHotbarUI();
    }

/// <summary>
/// Handles player input and opens or closes the inventory accordingly.
/// </summary>
/// <remarks>
/// This method checks for the space key press to toggle the visibility of the inventory canvas.
/// </remarks>
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (MenuManager.IsCurrentMenuClose(MenuID.INVENTORY))
            {
                // Close
                CloseInventory();
            }
            else if (MenuManager.CanOpenMenuSet(MenuID.INVENTORY))
            {
                // Open
                OpenInventory();
            }
        }


        if (Input.GetKeyDown(KeyCode.M))
        {
            for (int i = 0; i < data.items.Count; i++)
            {
                AddItem(data.items[i], 10000);
            }
        }
    }

/// <summary>
/// Adds an item to the player's inventory and updates the UI.
/// </summary>
/// <param name="item">The item to be added.</param>
/// <param name="count">The count of the item to be added.</param>
/// <remarks>
/// This method checks if the item is already in the inventory, increases its count, or adds a new entry.
/// It updates the UI if the inventory canvas is active.
/// </remarks>
    public void AddItem(Item item, int count)
    {
        if (inventory.ContainsKey(item))
        {
            // increase the amount held
            inventory[item] += count;
            if (hotbar[item])
                UpdateHotbarUI();
        }
        else
        { 
            inventory.Add(item, count);
            hotbar.Add(item, false);
        }

        if (inventoryCanvas.activeInHierarchy)
            UpdateUI();
    }
/// <summary>
/// Removes an item from the player's inventory and updates the UI.
/// </summary>
/// <param name="item">The item to be removed.</param>
/// <param name="count">The count of the item to be removed (default is -1 to remove completely).</param>
/// <param name="keepSelect">Indicates whether to keep the selected item in the hotbar.</param>
/// <remarks>
/// This method removes the specified item or decreases its count. It updates the UI accordingly.
/// </remarks>
    public void RemoveItem(Item item, int count = -1, bool keepSelect = false)
    {
        if (!inventory.ContainsKey(item))
            return;

        if (count == -1)
        {
            inventory.Remove(item);
            hotbar.Remove(item);
        }
        else
        {
            inventory[item] -= count;
            if (inventory[item] <= 0)
            {
                inventory.Remove(item);
                hotbar.Remove(item);
            }
        }

        if (inventoryCanvas.activeInHierarchy)
            UpdateUI();
        UpdateHotbarUI(keepSelect);
    }

/// <summary>
/// Updates the inventory UI based on the current inventory data.
/// </summary>
/// <remarks>
/// This method destroys existing UI elements, clears the list, and recreates UI elements for each inventory item.
/// </remarks>
    public void UpdateUI()
    {
        for (int i = 0; i < currentItems.Count; i++)
        {
            Destroy(currentItems[i]);
        }
        currentItems.Clear();

        foreach (KeyValuePair<Item, int> item in inventory)
        {
            var inventoryItem = Instantiate(itemPrefab, spawnLocation);
            currentItems.Add(inventoryItem);
            var itemUI = inventoryItem.GetComponent<InventoryItem>();
            itemUI.SetCount(item.Value.ToString());
            itemUI.SetIcon(data.items[item.Key.itemID].icon);
            itemUI.SaveInfo(item);
        }
    }
/// <summary>
/// Updates the hotbar UI based on the current hotbar data.
/// </summary>
/// <param name="keepSelected">Indicates whether to keep the selected item in the hotbar.</param>
/// <remarks>
/// This method destroys existing UI elements, clears the list, and recreates UI elements for each hotbar item.
/// </remarks>
    public void UpdateHotbarUI(bool keepSelected = true)
    {
        for (int i = 0; i < currentHotbarItems.Count; i++)
        {
            Destroy(currentHotbarItems[i]);
        }
        currentHotbarItems.Clear();

        int j = 0;
        foreach (KeyValuePair<Item, bool> item in hotbar)
        {
            if (item.Value)
            {
                var hotbarItem = Instantiate(hotbarItemPrefab, hotBarSpawnLocation);
                currentHotbarItems.Add(hotbarItem);

                var hotbarItemUi = hotbarItem.GetComponent<HotbarItem>();
                hotbarItemUi.SetCount(inventory[item.Key].ToString());
                hotbarItemUi.SetIcon(data.items[item.Key.itemID].icon);
                hotbarItemUi.SetID(j, player);
                j++;
            }
        }
        player.currentHotbarItems(hotbar, currentHotbarItems, keepSelected);
    }

/// <summary>
/// Pins or unpins an item to/from the hotbar and updates the UI.
/// </summary>
/// <param name="itemToPin">The item to pin or unpin.</param>
/// <remarks>
/// This method toggles the hotbar status of the specified item and updates the hotbar UI.
/// </remarks>
    public void PinToHotbar(Item itemToPin)
    {
        audioSource.PlayOneShot(pin);
        if (hotbar.ContainsKey(itemToPin))
        {
            hotbar.TryGetValue(itemToPin, out bool val);
            if (val)
                hotbar[itemToPin] = false;
            else
                hotbar[itemToPin] = true;
        }
        UpdateHotbarUI();
    }
/// <summary>
/// Plays the click sound effect.
/// </summary>
/// <remarks>
/// This method plays the click sound effect associated with inventory interactions.
/// </remarks>
    public void playClickSound()
    {
        audioSource.PlayOneShot(click);
    }
/// <summary>
/// Opens the inventory and updates the UI.
/// </summary>
/// <remarks>
/// This method sets the inventory canvas to active and updates the UI.
/// </remarks>
    public void OpenInventory()
    {
        audioSource.PlayOneShot(open);
        inventoryCanvas.SetActive(true);
        UpdateUI();
    }
/// <summary>
/// Closes the inventory.
/// </summary>
/// <remarks>
/// This method sets the inventory canvas to inactive.
/// </remarks>
    public void CloseInventory()
    {
        audioSource.PlayOneShot(close);
        inventoryCanvas.SetActive(false);
    }
}
