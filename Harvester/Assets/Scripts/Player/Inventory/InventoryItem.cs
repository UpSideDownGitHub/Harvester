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

    public void Awake()
    {
        info = GameObject.FindGameObjectWithTag("InventoryInfo").GetComponent<InventoryInfo>();
    }
    public void SetInfo()
    {
        info.SetName(data.items[item.Key.itemID].itemName);
        info.SetIcon(data.items[item.Key.itemID].icon);
        info.SetDescription(data.items[item.Key.itemID].description);
        info.SetItem(item.Key);
    }
    public void SaveInfo(KeyValuePair<Item, int> givenItem)
    {
        item = givenItem;
    }
    public void SetCount(string count)
    {
        this.count.text = count;
    }
    public void SetIcon(Sprite icon)
    {
        this.icon.sprite = icon;
    }
}
