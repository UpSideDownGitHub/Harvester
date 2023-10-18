using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingStationObject : MonoBehaviour
{
    [Header("Inventory")]
    public Inventory inventory;

    [Header("World Object")]
    public GameObject UI;
    public Button closeMenuButton;
    public bool inRange;

    [Header("Station ID")]
    public CraftingStationData stationData;
    public int stationID;

    [Header("UI")]
    public TMP_Text stationName;
    public TMP_Text craftCount;
    public int currentCraftCount;
    public int currentSelectedRecipieID;

    [Header("Crafting Station Recipe")]
    public Transform recipeTransform;
    public GameObject itemRecipePrefab;

    [Header("Crafting Station Info")]
    public TMP_Text itemName;
    public Image icon;
    public Transform itemAmountParent;
    public GameObject itemAmountPrefab;

    void Start()
    {
        inventory = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<Inventory>();

        // spawn and set all of the information
        // station name
        stationName.text = stationData.stations[stationID].stationName;

        for (int i = 0; i < recipeTransform.childCount; i++)
        {
            Destroy(recipeTransform.GetChild(i).gameObject);
        }

        // station recipies
        for (int i = 0; i < stationData.stations[stationID].recipies.Length; i++)
        {
            var spawnedObject = Instantiate(itemRecipePrefab, recipeTransform);
            var recipieItem = spawnedObject.GetComponent<CraftingStationRecipeItem>();
            recipieItem.itemName.text = stationData.stations[stationID].recipies[i].recipeName;
            recipieItem.icon.sprite = stationData.stations[stationID].recipies[i].produces.item.icon;
            recipieItem.ID = i;
            recipieItem.craftingManager = this;
        }

        closeMenuButton.onClick.AddListener(() => CloseMenu());
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && inRange)
        {
            if (UI.activeInHierarchy)
                UI.SetActive(false);
            else
                UI.SetActive(true);
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
        UI.SetActive(true);
    }
    public void CloseMenu()
    {
        UI.SetActive(false);
    }

    public void ItemPressed(int ID, int count = 1)
    {
        currentCraftCount = count;
        currentSelectedRecipieID = ID;
        for (int i = 0; i < itemAmountParent.childCount; i++)
        {
            Destroy(itemAmountParent.GetChild(i).gameObject);
        }

        // Name & Icon
        itemName.text = stationData.stations[stationID].recipies[ID].recipeName;
        icon.sprite = stationData.stations[stationID].recipies[ID].produces.item.icon;

        for (int i = 0; i < stationData.stations[stationID].recipies[ID].materials.Length; i++)
        {
            var spawnedObject = Instantiate(itemAmountPrefab, itemAmountParent);
            var infoItem = spawnedObject.GetComponent<CraftingStationInfoItem>();
            infoItem.icon.sprite = stationData.stations[stationID].recipies[ID].materials[i].item.icon;
            infoItem.itemName.text = stationData.stations[stationID].recipies[ID].materials[i].item.itemName;
            var neededCount = stationData.stations[stationID].recipies[ID].materials[i].count;

            var haveCount = inventory.inventory.ContainsKey(stationData.stations[stationID].recipies[ID].materials[i].item) ? 
                inventory.inventory[stationData.stations[stationID].recipies[ID].materials[i].item] : 0;

            infoItem.itemCount.text = haveCount + "/" + neededCount * count;
        }
    }

    public void Craft()
    {
        if (currentCraftCount > MaxCraftable() || currentCraftCount <= 0)
            return;

        // take off the given ammount of materials
        for (int i = 0; i < stationData.stations[stationID].recipies[currentSelectedRecipieID].materials.Length; i++)
        {
            inventory.RemoveItem(stationData.stations[stationID].recipies[currentSelectedRecipieID].materials[i].item,
                stationData.stations[stationID].recipies[currentSelectedRecipieID].materials[i].count * currentCraftCount);
        }

        // give the player the crafted amount of items
        inventory.AddItem(stationData.stations[stationID].recipies[currentSelectedRecipieID].produces.item,
            stationData.stations[stationID].recipies[currentSelectedRecipieID].produces.count * currentCraftCount);
        
    }

    public void SetToOne()
    {
        currentCraftCount = 1;
        craftCount.text = "x" + currentCraftCount.ToString();
        ItemPressed(currentSelectedRecipieID, currentCraftCount);
    }
    public void SetMax()
    {
        var smallest = MaxCraftable();
        currentCraftCount = smallest <= 0 ? 1 : smallest;
        craftCount.text = "x" + currentCraftCount.ToString();
        ItemPressed(currentSelectedRecipieID, currentCraftCount);
    }
    public void IncreaseCraft()
    {
        currentCraftCount++;
        craftCount.text = "x" + currentCraftCount.ToString();
        ItemPressed(currentSelectedRecipieID, currentCraftCount);
    }
    public void DecreaseCraft()
    {

        currentCraftCount = currentCraftCount - 1 <= 0 ? 1 : currentCraftCount - 1;
        craftCount.text = "x" + currentCraftCount.ToString();
        ItemPressed(currentSelectedRecipieID, currentCraftCount);
    }

    /// <summary>
    /// return the maximum craftable of the current selected recipie
    /// </summary>
    /// <returns>smallest - whichever ingredient is lacking the most as this will effect the max amount craftable</returns>
    public int MaxCraftable()
    {
        var smallest = 9999;
        for (int i = 0; i < stationData.stations[stationID].recipies[currentSelectedRecipieID].materials.Length; i++)
        {
            var neededCount = stationData.stations[stationID].recipies[currentSelectedRecipieID].materials[i].count;
            var haveCount = inventory.inventory.ContainsKey(stationData.stations[stationID].recipies[currentSelectedRecipieID].materials[i].item) ?
                inventory.inventory[stationData.stations[stationID].recipies[currentSelectedRecipieID].materials[i].item] : 0;

            smallest = haveCount / neededCount < smallest ? haveCount / neededCount : smallest;
        }
        return smallest;
    }
}
