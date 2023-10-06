using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Dictionary<Item, int> inventory = new();

    [Header("UI")]
    public GameObject inventoryCanvas;
    public List<int> equippedItems = new();
    public List<GameObject> currentItems = new();
    public Transform spawnLocation;
    public GameObject itemPrefab;

    [Header("Data")]
    public ItemData data;


    // DELETE ME THIS IS FOR TESTING
    public void Start()
    {
        AddItem(data.items[0], 10);
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
        }
        else
            inventory.Add(item, count);

        if (inventoryCanvas.activeInHierarchy)
            UpdateUI();
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
            itemUI.SetIcon(data.items[item.Key.ID].icon);
            itemUI.SaveInfo(item);
        }
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
