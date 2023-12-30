using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarItem : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text count;
    public Image icon;

    [Header("Hotbar")]
    public Button button;
    public int hotbarID;

/// <summary>
/// Sets the ID of the hotbar item and assigns a button click listener to notify the player of the selection.
/// </summary>
/// <param name="ID">The ID of the hotbar item.</param>
/// <param name="player">The player associated with the hotbar.</param>
/// <remarks>
/// This method sets the hotbarID and adds a button click listener to trigger the SetSelected method on the player when clicked.
/// </remarks>
    public void SetID(int ID, Player player)
    {
        hotbarID = ID;
        button.onClick.AddListener(() => player.SetSelected(hotbarID));
    }    
/// <summary>
/// Sets the displayed count for the hotbar item.
/// </summary>
/// <param name="count">The count value to be displayed.</param>
/// <remarks>
/// This method updates the count text on the hotbar item.
/// </remarks>
    public void SetCount(string count)
    {
        this.count.text = count;
    }
/// <summary>
/// Sets the icon for the hotbar item.
/// </summary>
/// <param name="icon">The sprite representing the item icon.</param>
/// <remarks>
/// This method updates the icon sprite on the hotbar item.
/// </remarks>
    public void SetIcon(Sprite icon)
    {
        this.icon.sprite = icon;
    }
}
