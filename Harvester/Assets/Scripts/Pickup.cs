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

    [SyncVar] public bool destroy = false;

    public void SetPickup(Item item, int count, Sprite icon)
    {
        this.item = item;
        this.count = count;
        spriteRenderer.sprite = icon;
    }

    void Update()
    {
        if (destroy)
            Destroy(gameObject);

        if (inRange)
        { 
            transform.position = Vector3.Lerp(transform.position, player.transform.position, lerpSpeed); 
            var dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist < pickupDistance)
            {
                player.GetComponent<Player>().inventory.AddItem(item, count);
                enabled = false;
                PickupUsed(this, true);
            }
        }
    }

    [ServerRpc]
    public void PickupUsed(Pickup script, bool isDestroyed)
    {
        this.destroy = isDestroyed;
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
