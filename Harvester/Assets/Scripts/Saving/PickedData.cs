using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Data Objects/Picked Data")]
public class PickedData : ScriptableObject
{
    public int mapID;
    public int playerID;
}
