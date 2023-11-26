using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player")]
    public Rigidbody2D rb;
    public float speed;
    public Player player;

    [Header("Camera")]
    public Camera cam;
    public float lerpSpeed;
    public Vector3 offset;

    public void Start()
    {
        cam = Camera.main;    
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!PhotonView.Get(this).IsMine)
            return;
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
