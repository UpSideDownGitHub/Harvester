using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    public float speed;

    // Update is called once per frame
    void FixedUpdate()
    {
        var hor = Input.GetAxis("Horizontal");
        var ver = Input.GetAxis("Vertical");
        Vector2 movement = new Vector2(hor, ver);

        rb.AddForce(movement * speed * Time.deltaTime);
    }
}
