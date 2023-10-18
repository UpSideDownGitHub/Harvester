using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = System.Object;

[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    private ItemData targetComponent;

    public List<Item> _attributeSet;

    public override void OnInspectorGUI()
    {
        targetComponent = (ItemData)target;

        base.OnInspectorGUI();
        if (GUILayout.Button("Get Items"))
        {
            List<Item> tempList = new List<Item>();
            for (int i = 0; i < targetComponent.items.Count; i++)
            {
                tempList.Add(targetComponent.items[i]);
            }

            for (int i = 0; i < targetComponent.items.Count; i++)
            {
                tempList[targetComponent.items[i].itemID] = targetComponent.items[i];
            }
            targetComponent.items = tempList;
        }
    }
}
