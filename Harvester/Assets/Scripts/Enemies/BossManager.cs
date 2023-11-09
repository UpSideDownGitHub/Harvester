using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using NavMeshPlus.Components;

public class BossManager : NetworkBehaviour
{
    public string bossTag;

    public bool isBossAlive()
    {
        return GameObject.FindGameObjectWithTag(bossTag) != null ? true : false;
    }
}
