using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
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
                player.GetComponent<Player>().inventory.AddItem(item, count);
                Destroy(gameObject);
            }
        }
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
