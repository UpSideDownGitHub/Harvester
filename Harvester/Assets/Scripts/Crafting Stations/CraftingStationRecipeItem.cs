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

    /// <summary>
    /// Initializes a button click event to trigger the associated method in the CraftingManager when the button is clicked.
    /// </summary>
    public void Start()
    {
        button.onClick.AddListener(() => craftingManager.ItemPressed(ID));
    }
}