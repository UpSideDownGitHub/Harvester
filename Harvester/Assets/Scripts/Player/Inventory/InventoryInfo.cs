using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
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

    public void SetItem(Item givenItem)
    {
        item = givenItem;
    }

    public void PinPressed()
    {
        inventory.PinToHotbar(item);
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
