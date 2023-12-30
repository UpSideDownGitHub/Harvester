using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon.StructWrapping;

public class PlaceableObject : MonoBehaviour
{
    [Header("Personal Data")]
    public Placeable placeable;
    public float currentHealth;
    public GameObject pickupItem;

    [Header("UI")]
    public GameObject UICanvas;
    public Slider healthSlider;

    [Header("Crafting Station")]
    public bool isCraftingStation;
    public CraftingStationObject craftingStation;

    [Header("Farm")]
    public bool isFarm;
    public FarmObject farm;

    [Header("Nature Item")]
    public borders[] areaBorders;
    public bool isNature;
    //public SpawnAreas spawnAreas;
    public ItemSpawner spawner;
    public int spawnAreaID;

    [Header("Better Item Spawning")]
    public float itemSpawnRange = 0.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip destroyed;

/// <summary>
/// Initializes the placeable object, setting up references and determining the spawn area ID based on its location.
/// </summary>
/// <remarks>
/// This method is called when the placeable object is created. It sets up necessary references,
/// initializes health-related properties, and determines the spawn area ID based on the object's location.
/// </remarks>
    public void Start()
    {
        spawner = GameObject.FindGameObjectWithTag("ItemSpawner").GetComponent<ItemSpawner>();
        audioSource = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioSource>();

        currentHealth = placeable.health;
        healthSlider.minValue = 0;
        healthSlider.maxValue = placeable.health;
        healthSlider.value = healthSlider.maxValue;

        // find ther location and thius the spawn area ID
        //print(transform.position);
        for (int k = 0; k < areaBorders.Length; k++)
        {
            if (inRange(transform.position, areaBorders[k].TL.x, areaBorders[k].BR.x, areaBorders[k].BR.y, areaBorders[k].TL.y))
            {
                spawnAreaID = k;
                break;
            }
        }
    }

/// <summary>
/// Checks if a given position is within a specified range.
/// </summary>
/// <param name="pos">The position to check.</param>
/// <param name="xMin">Minimum X coordinate of the range.</param>
/// <param name="xMax">Maximum X coordinate of the range.</param>
/// <param name="yMin">Minimum Y coordinate of the range.</param>
/// <param name="yMax">Maximum Y coordinate of the range.</param>
/// <returns>True if the position is within the specified range, otherwise false.</returns>
/// <remarks>
/// This method checks if a given position is within a specified range defined by minimum and maximum X and Y coordinates.
/// </remarks>
    bool inRange(Vector3 pos, int xMin = -100, int xMax = 100, int yMin = -100, int yMax = 100) =>
            ((pos.x - xMin) * (pos.x - xMax) <= 0) && ((pos.y - yMin) * (pos.y - yMax) <= 0);

/// <summary>
/// Sets the name of the placeable object.
/// </summary>
/// <param name="givenName">The name to set for the placeable object.</param>
/// <remarks>
/// This method is a PunRPC (Photon Unity Networking Remote Procedure Call) that sets the name of the placeable object.
/// </remarks>
    [PunRPC]
    public void SetName(string givenName)
    {
        gameObject.name = givenName;
    }

/// <summary>
/// Handles taking damage on the placeable object, updating health and triggering destruction if health reaches zero.
/// </summary>
/// <param name="damage">The amount of damage to apply to the placeable object.</param>
/// <remarks>
/// This method is a PunRPC (Photon Unity Networking Remote Procedure Call) that handles taking damage on the placeable object.
/// It updates the current health, triggers UI display if not already active, and destroys the object if health reaches zero.
/// </remarks>
    [PunRPC]
    public void TakeDamage(float damage)
    {
        if (isCraftingStation)
        {
            if (craftingStation.crafting)
                return;
        }

        if (!UICanvas.activeInHierarchy)
            UICanvas.SetActive(true);

        currentHealth = currentHealth - damage < 0 ? 0 : currentHealth - damage;
        healthSlider.value = currentHealth;
        if (currentHealth == 0)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            audioSource.PlayOneShot(destroyed);

            if (isNature)
            {
                spawner.currentSpawns[spawnAreaID] = spawner.currentSpawns[spawnAreaID] - 1 <= 0 ? 0 : spawner.currentSpawns[spawnAreaID] - 1;
            }

            if (isFarm)
            {
                for (int i = 0; i < farm.count.Length; i++)
                {
                    var spawnPosition = new Vector2(transform.position.x + Random.Range(-itemSpawnRange, itemSpawnRange),
                        transform.position.y + Random.Range(-itemSpawnRange, itemSpawnRange));
                    GameObject drop = PhotonNetwork.Instantiate(pickupItem.name, spawnPosition, Quaternion.identity, 0);

                    PhotonView photonView1 = PhotonView.Get(drop);
                    photonView1.RPC("SetPickup", RpcTarget.All, farm.farmData.Farms[farm.farmID].items[i].item.itemID, farm.count[i]);
                    farm.count[i] = 0;
                }
            }

            for (int i = 0; i < placeable.drops.Length; i++)
            {
                var spawnPosition = new Vector2(transform.position.x + Random.Range(-itemSpawnRange, itemSpawnRange),
                        transform.position.y + Random.Range(-itemSpawnRange, itemSpawnRange));
                GameObject drop = PhotonNetwork.Instantiate(pickupItem.name, spawnPosition, Quaternion.identity, 0);

                PhotonView photonView2 = PhotonView.Get(drop);
                photonView2.RPC("SetPickup", RpcTarget.All, placeable.drops[i].item.itemID, placeable.drops[i].count);
            }

            GridManager gridManager = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>();
            PhotonView photonView3 = PhotonView.Get(gridManager);
            photonView3.RPC("RemoveObject", RpcTarget.All, transform.position);


            PhotonView view = PhotonView.Get(this);
            view.RPC("DestroyObject", RpcTarget.All);
        }
    }

/// <summary>
/// Destroys the placeable object, triggering network destruction if the view is owned by the local player.
/// </summary>
/// <remarks>
/// This method is a PunRPC (Photon Unity Networking Remote Procedure Call) that destroys the placeable object.
/// It triggers network destruction only if the view is owned by the local player.
/// </remarks>
    [PunRPC]
    public void DestroyObject()
    {
        PhotonView view = PhotonView.Get(gameObject);
        if (view.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }
}
