using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Data Objects/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public string description;
    public int itemID;
    public Sprite icon;
    
    // placeable
    public bool placeable;
    public int placeableObjectID;

    // consumable
    public bool consumable;
    public int consumableObjectID;

    // tool
    public bool tool;
    public int toolID;
    public int toolLevel;


    public bool boss;
    public int bossID;
}
