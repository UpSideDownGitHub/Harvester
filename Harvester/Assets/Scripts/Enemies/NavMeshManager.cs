using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NavMeshPlus.Components;
using Photon.Pun;
using UnityEngine.AI;
public class NavMeshManager : MonoBehaviour
{
    public NavMeshSurface Surface2D;

/// <summary>
/// RPC method to update the 2D navigation mesh.
/// </summary>
    [PunRPC]
    public void UpdateNavMesh()
    {
        Surface2D.UpdateNavMesh(Surface2D.navMeshData);
    }

/// <summary>
/// Retrieves the closest position on the NavMesh to the specified spawn position.
/// </summary>
/// <param name="spawnPos">The position to find the closest point on the NavMesh.</param>
/// <returns>The closest position on the NavMesh to the specified spawn position.</returns>

    public Vector3 GetClosestNavMeshPosition(Vector3 spawnPos)
    {
        NavMeshHit navHit;
        NavMesh.SamplePosition(spawnPos, out navHit, 9999, -1);
        return navHit.position;
    }
}