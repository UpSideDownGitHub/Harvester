using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* The CraftingStationInfoItem class represents an item in a crafting station with UI elements such as
an icon, item name, and item count. */
public class CraftingStationInfoItem : MonoBehaviour
{
    [Header("UI")]
    public Image icon;
    public TMP_Text itemName;
    public TMP_Text itemCount;
}
