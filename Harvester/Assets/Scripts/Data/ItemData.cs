using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Data Objects/Items")]
public class ItemData : ScriptableObject
{
    public List<Item> items;
}
[Serializable]
[CreateAssetMenu(menuName = "Data Objects/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public int ID;
    public Texture2D icon;
    public bool placeAble;
    public int placeableID;

    public void SetCustomIcon()
    {
        EditorGUIUtility.SetIconForObject(this, icon);
    }
}
