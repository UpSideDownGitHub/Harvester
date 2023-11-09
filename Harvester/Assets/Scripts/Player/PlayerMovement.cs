using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Example.Scened;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Player")]
    public Rigidbody2D rb;
    public float speed;
    public Player player;

    [Header("Camera")]
    public Camera cam;
    public float lerpSpeed;
    public Vector3 offset;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            cam = Camera.main;
        }
        else
        {
            gameObject.GetComponent<PlayerMovement>().enabled = false;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!cam || player.dead)
            return;

        var hor = Input.GetAxis("Horizontal");
        var ver = Input.GetAxis("Vertical");
        Vector2 movement = new Vector2(hor, ver);

        rb.AddForce(movement * speed * Time.deltaTime);

        Vector3 targetPosition = new Vector3(transform.position.x + offset.x,
            transform.position.y + offset.y,
            transform.position.z + offset.z);
        cam.transform.position = Vector3.Slerp(targetPosition, cam.transform.position, lerpSpeed);
    }
}
