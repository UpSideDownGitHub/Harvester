using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NavMeshPlus.Components;
using Photon.Pun;
using UnityEngine.AI;
public class NavMeshManager : MonoBehaviour
{
    public NavMeshSurface Surface2D;

    [PunRPC]
    public void UpdateNavMesh()
    {
        Surface2D.UpdateNavMesh(Surface2D.navMeshData);
    }

    public Vector3 GetClosestNavMeshPosition(Vector3 spawnPos)
    {
        NavMeshHit navHit;
        NavMesh.SamplePosition(spawnPos, out navHit, 9999, -1);
        return navHit.position;
    }
}