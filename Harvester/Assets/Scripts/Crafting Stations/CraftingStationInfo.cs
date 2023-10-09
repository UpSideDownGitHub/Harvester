using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CraftingStationInfo : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text itemName;
    public SpriteRenderer icon;
    public Transform itemAmountParent;
    public GameObject itemAmountPrefab;

}
