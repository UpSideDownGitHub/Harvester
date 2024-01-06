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

    /// <summary>
    /// Sets the pickup object with the specified item and count, updating its appearance and properties.
    /// </summary>
    /// <param name="item">The ID of the item to assign to the pickup object.</param>
    /// <param name="count">The count of the item to assign to the pickup object.</param>
    /// <remarks>
    /// This method is a PunRPC (Photon Unity Networking Remote Procedure Call) that sets the pickup object with the specified item and count.
    /// It updates the pickup object's item, count, and sprite, making it visually represent the assigned item.
    /// </remarks>
    [PunRPC]
    public void SetPickup(int item, int count)
    {
        this.item = items.items[item];
        this.count = count;
        spriteRenderer.sprite = items.items[item].icon;
    }

    /// <summary>
    /// Initializes the pickup object by setting the time of spawn.
    /// </summary>
    /// <remarks>
    /// This method initializes the pickup object by setting the time of spawn to the current time.
    /// It is typically called when the pickup object is instantiated.
    /// </remarks>
    public void Start()
    {
        _timeOfSpawn = Time.time;
    }

    /// <summary>
    /// Updates the pickup object's behavior, moving towards the player and triggering item pickup when in range.
    /// </summary>
    /// <remarks>
    /// This method updates the pickup object's behavior, moving towards the player and triggering item pickup when in range.
    /// It uses Photon RPCs to synchronize the item pickup action among all clients in a multiplayer environment.
    /// </remarks>
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
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

    /// <summary>
    /// Destroys the pickup object, removing it from the scene.
    /// </summary>
    /// <remarks>
    /// This method is a PunRPC (Photon Unity Networking Remote Procedure Call) that destroys the pickup object, removing it from the scene.
    /// It is typically called after the item has been successfully picked up by a player.
    /// </remarks>
    [PunRPC]
    public void DestroyObject()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Handles the behavior when another Collider enters the trigger zone of the pickup object.
    /// </summary>
    /// <param name="collision">The Collider of the object entering the trigger zone.</param>
    /// <remarks>
    /// This method is called when another Collider enters the trigger zone of the pickup object.
    /// It sets the player variable and triggers the inRange flag, indicating that the pickup object is in range of a player.
    /// </remarks>
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player = collision.gameObject;
            inRange = true;
        }
    }
}