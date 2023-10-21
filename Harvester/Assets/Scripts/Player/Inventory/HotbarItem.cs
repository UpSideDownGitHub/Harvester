using FishNet.Object;
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

    public void SetID(int ID, Player player)
    {
        hotbarID = ID;
        button.onClick.AddListener(() => player.SetSelected(hotbarID));
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
