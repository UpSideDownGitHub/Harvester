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
