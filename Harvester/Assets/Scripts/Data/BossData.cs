using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Data Objects/Boss")]
public class BossData : ScriptableObject
{
    public List<Boss> bosses;
}
[Serializable]
public struct Boss
{
    public string bossName;
    public int bossID;
    public GameObject bossObject;
}
