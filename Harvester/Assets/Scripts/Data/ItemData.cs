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
