using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventoryInfo : MonoBehaviour
{
    public TMP_Text objectName;
    public Image icon;
    public TMP_Text description;

    [Header("Hotbar")]
    public Inventory inventory;
    private Item item;

    [Header("Quick Pin0")]
    public bool canPin;

/// <summary>
/// Handles player input for pinning an item to the hotbar.
/// </summary>
/// <remarks>
/// This method checks for the Q key press and, if allowed, triggers the PinPressed method.
/// </remarks>
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && canPin)
            PinPressed();
    }

/// <summary>
/// Sets the item for the inventory item.
/// </summary>
/// <param name="givenItem">The item to be set.</param>
/// <remarks>
/// This method sets the item, enables pinning, and plays a click sound effect.
/// </remarks>
    public void SetItem(Item givenItem)
    {
        item = givenItem;
        canPin = true;
        inventory.playClickSound();
    }

/// <summary>
/// Handles the pinning of an item to the hotbar.
/// </summary>
/// <remarks>
/// This method triggers the PinToHotbar method in the inventory, effectively pinning the associated item.
/// </remarks>
    public void PinPressed()
    {
        inventory.PinToHotbar(item);
    }
/// <summary>
/// Deletes the associated item from the inventory.
/// </summary>
/// <remarks>
/// This method removes the associated item from the inventory using the RemoveItem method.
/// </remarks>
    public void DeleteItem()
    {
        inventory.RemoveItem(item);
    }
/// <summary>
/// Sets the name text for the inventory item.
/// </summary>
/// <param name="name">The name to be set.</param>
/// <remarks>
/// This method sets the name text for the inventory item UI.
/// </remarks>
    public void SetName(string name)
    {
        objectName.text = name;
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
/// <summary>
/// Sets the description text for the inventory item.
/// </summary>
/// <param name="desc">The description to be set.</param>
/// <remarks>
/// This method sets the description text for the inventory item UI.
/// </remarks>
    public void SetDescription(string desc)
    {
        description.text = desc;
    }
}
