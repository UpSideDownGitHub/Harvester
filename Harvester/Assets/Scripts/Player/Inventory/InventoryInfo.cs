using FishNet.Object;
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

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && canPin)
            PinPressed();
    }

    public void SetItem(Item givenItem)
    {
        item = givenItem;
        canPin = true;
    }

    public void PinPressed()
    {
        inventory.PinToHotbar(item);
    }
    public void DeleteItem()
    {
        inventory.RemoveItem(item);
    }
    public void SetName(string name)
    {
        objectName.text = name;
    }
    public void SetIcon(Sprite icon)
    {
        this.icon.sprite = icon;
    }
    public void SetDescription(string desc)
    {
        description.text = desc;
    }
}
