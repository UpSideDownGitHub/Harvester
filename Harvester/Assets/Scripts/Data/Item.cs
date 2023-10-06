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
    public int ID;
    public Sprite icon;
    public bool placeable;
    public int placeableObjectID;
    public string description;
}
