using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class Pickup : NetworkBehaviour
{
    [Header("Pickup Info")]
    public Item item;
    public int count;

    [Header("Movement Information")]
    public bool inRange;
    public float lerpSpeed;
    public GameObject player;
    public float pickupDistance;

    [Header("Sprite")]
    public SpriteRenderer spriteRenderer;

    public void SetPickup(Item item, int count, Sprite icon)
    {
        this.item = item;
        this.count = count;
        spriteRenderer.sprite = icon;
    }

    void Update()
    {
        if (inRange)
        { 
            transform.position = Vector3.Lerp(transform.position, player.transform.position, lerpSpeed); 
            var dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist < pickupDistance)
            {
                print("ADDED THE OBJCT TO INVENTROY: " + count);
                inRange = false;
                DestroyItem(this, gameObject);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyItem(Pickup script, GameObject ojectToDestroy)
    {
        script.player.GetComponent<Player>().inventory.AddItem(item, count);
        ServerManager.Despawn(ojectToDestroy);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player = collision.gameObject;
            inRange = true;
        }
    }
}
