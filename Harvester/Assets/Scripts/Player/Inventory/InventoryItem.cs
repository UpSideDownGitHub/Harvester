using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class InventoryItem : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text count;
    public Image icon;

    [Header("Info")]
    public InventoryInfo info;
    public ItemData data;

    private KeyValuePair<Item, int> item;

    public void Start()
    {
        info = GameObject.FindGameObjectWithTag("InventoryInfo").GetComponent<InventoryInfo>();
    }
    public void SetInfo()
    {
        info.SetName(data.items[item.Key.ID].itemName);
        info.SetIcon(data.items[item.Key.ID].icon);
        info.SetDescription(data.items[item.Key.ID].description);
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
