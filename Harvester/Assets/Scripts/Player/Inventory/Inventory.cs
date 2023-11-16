using FishNet.Object;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
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

    [Header("Save Data")]
    public PickedData pickedData;

    public bool isInventoryOpen()
    {
        return inventoryCanvas.activeInHierarchy;
    } 

    public void SavePlayerData()
    {
        // Save the current map data to the file
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
        saveData.players[pickedData.mapID] = playerData;
        SaveManager.instance.SavePlayerData(saveData);
    }

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

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            ToggleInventory();

        
        if (Input.GetKeyDown(KeyCode.M))
        {
            for (int i = 0; i < data.items.Count; i++)
            {
                AddItem(data.items[i], 10000);
            }
        }
    }

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

    public void PinToHotbar(Item itemToPin)
    {
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

    public void ToggleInventory()
    {
        if (inventoryCanvas.activeInHierarchy)
            CloseInventory();
        else
            OpenInventory();
    }
    public void OpenInventory()
    {
        if (craftingCanvas.activeInHierarchy)
            return;

        inventoryCanvas.SetActive(true);
        UpdateUI();
    }
    public void CloseInventory()
    {
        inventoryCanvas.SetActive(false);
    }
}
