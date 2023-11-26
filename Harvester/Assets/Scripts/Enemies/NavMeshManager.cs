using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NavMeshPlus.Components;
using Photon.Pun;

public class NavMeshManager : MonoBehaviour
{
    public NavMeshSurface Surface2D;

    [PunRPC]
    public void UpdateNavMesh()
    {
        Surface2D.UpdateNavMesh(Surface2D.navMeshData);
    }
}
