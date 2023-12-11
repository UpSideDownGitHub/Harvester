using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Security.Policy;

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

    [Header("Synced info")]
    public ItemData items;
    public int[] info;

    [Header("WaitTime")]
    public float waitTime;
    private float _timeOfSpawn;

    [PunRPC]
    public void SetPickup(int item, int count)
    {
        this.item = items.items[item];
        this.count = count;
        spriteRenderer.sprite = items.items[item].icon;
    }

    public void Start()
    {
        _timeOfSpawn = Time.time;
    }

    void Update()
    {
        if (inRange && Time.time > _timeOfSpawn + waitTime)
        { 
            transform.position = Vector3.Lerp(transform.position, player.transform.position, lerpSpeed); 
            var dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist < pickupDistance)
            {

                //print("ADDED THE OBJCT TO INVENTORY: " + count);
                inRange = false;

                PhotonView playerView = PhotonView.Get(player);
                playerView.RPC("AddItemToInventory", RpcTarget.All, item.itemID, count);

                PhotonView view = PhotonView.Get(this);
                view.RPC("DestroyObject", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    public void DestroyObject()
    {
        Destroy(gameObject);
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
