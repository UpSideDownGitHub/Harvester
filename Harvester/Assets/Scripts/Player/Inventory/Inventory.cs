using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class Inventory : MonoBehaviour
{
    public Dictionary<Item, int> inventory = new();
    public Dictionary<Item, bool> hotbar = new();

    [Header("UI")]
    public GameObject inventoryCanvas;
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
    
    public bool isInventoryOpen()
    {
        return inventoryCanvas.activeInHierarchy;
    }


    // DELETE ME THIS IS FOR TESTING
    public void Start()
    {
        AddItem(data.items[0], 100);
        AddItem(data.items[1], 10);
        AddItem(data.items[2], 20);
        AddItem(data.items[3], 1);
        AddItem(data.items[4], 10);
        AddItem(data.items[5], 1);
        AddItem(data.items[6], 1);
        AddItem(data.items[7], 1);
        AddItem(data.items[8], 1);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            AddItem(data.items[1], 10);
        if (Input.GetKeyDown(KeyCode.O))
            AddItem(data.items[0], 1);
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
    public void RemoveItem(Item item, int count, bool keepSelect = false)
    {
        if (!inventory.ContainsKey(item))
            return;

        inventory[item] -= count;
        if (inventory[item] <= 0)
        {
            inventory.Remove(item);
            hotbar.Remove(item);
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
    public void UpdateHotbarUI(bool keepSelected = false)
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
        inventoryCanvas.SetActive(true);
        UpdateUI();
    }
    public void CloseInventory()
    {
        inventoryCanvas.SetActive(false);
    }
}
