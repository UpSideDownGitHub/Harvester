using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public GameObject player;
    public float lerpSpeed;
    public Vector3 offset;

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 targetPosition = new Vector3(player.transform.position.x + offset.x,
            player.transform.position.y + offset.y,
            transform.position.z + offset.z);
        transform.position = Vector3.Slerp(targetPosition, transform.position, lerpSpeed);
    }
}
