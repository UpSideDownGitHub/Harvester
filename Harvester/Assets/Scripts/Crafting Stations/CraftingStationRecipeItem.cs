using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingStationRecipeItem : MonoBehaviour
{
    [Header("UI")]
    public Image icon;
    public TMP_Text itemName;
    public int ID;

    [Header("Button")]
    public Button button;
    public CraftingStationObject craftingManager;

    public void Start()
    {
        button.onClick.AddListener(() => craftingManager.ItemPressed(ID));
    }
}