using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using NavMeshPlus.Components;

public class NavMeshManager : NetworkBehaviour
{
    [SyncVar(OnChange = "")] public bool navMeshUpdated;
    public NavMeshSurface Surface2D;

    public void navMeshUpdate(bool preValue, bool newValue, bool asServer)
    {
        if (asServer)
            return;

        Surface2D.BuildNavMeshAsync();
    }

    public void UpdateNavMesh()
    {
        Surface2D.BuildNavMeshAsync();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerUpdate()
    {
        navMeshUpdated = !navMeshUpdated;
    }
}
