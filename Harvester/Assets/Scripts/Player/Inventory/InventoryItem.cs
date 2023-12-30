using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text count;
    public Image icon;

    [Header("Info")]
    public InventoryInfo info;
    public ItemData data;

    private KeyValuePair<Item, int> item;

/// <summary>
/// Initializes the InventoryInfo component reference during Awake.
/// </summary>
/// <remarks>
/// This method retrieves the InventoryInfo component using a tag and stores it for future use.
/// </remarks>
    public void Awake()
    {
        info = GameObject.FindGameObjectWithTag("InventoryInfo").GetComponent<InventoryInfo>();
    }
/// <summary>
/// Sets the information in the InventoryInfo UI based on the associated item.
/// </summary>
/// <remarks>
/// This method calls various methods in the InventoryInfo component to set the name, icon, description, and item for the associated item.
/// </remarks>
    public void SetInfo()
    {
        info.SetName(data.items[item.Key.itemID].itemName);
        info.SetIcon(data.items[item.Key.itemID].icon);
        info.SetDescription(data.items[item.Key.itemID].description);
        info.SetItem(item.Key);
    }
/// <summary>
/// Saves the information for the associated item.
/// </summary>
/// <param name="givenItem">The item to be associated with.</param>
/// <remarks>
/// This method stores the key-value pair representing the item and its count.
/// </remarks>
    public void SaveInfo(KeyValuePair<Item, int> givenItem)
    {
        item = givenItem;
    }
/// <summary>
/// Sets the count text for the inventory item.
/// </summary>
/// <param name="count">The count to be set.</param>
/// <remarks>
/// This method sets the count text for the inventory item UI.
/// </remarks>
    public void SetCount(string count)
    {
        this.count.text = count;
    }
/// <summary>
/// Sets the icon for the inventory item.
/// </summary>
/// <param name="icon">The icon to be set.</param>
/// <remarks>
/// This method sets the icon for the inventory item UI.
/// </remarks>
    public void SetIcon(Sprite icon)
    {
        this.icon.sprite = icon;
    }
}
