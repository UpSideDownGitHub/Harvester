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
    public SpawnAreas spawnAreas;
    public int spawnAreaID;

    public void Start()
    {
        currentHealth = placeable.health;
        healthSlider.minValue = 0;
        healthSlider.maxValue = placeable.health;
        healthSlider.value = healthSlider.maxValue;

        // find ther location and thius the spawn area ID
        for (int k = 0; k < areaBorders.Length; k++)
        {
            if (inRange(transform.position, areaBorders[k].TL.x, areaBorders[k].BR.x, areaBorders[k].BR.y, areaBorders[k].TL.y))
                spawnAreaID = k;
        }
    }

    bool inRange(Vector3 pos, int xMin = -100, int xMax = 100, int yMin = -100, int yMax = 100) =>
            ((pos.x - xMin) * (pos.x - xMax) <= 0) && ((pos.y - yMin) * (pos.y - yMax) <= 0);


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
            if (isNature)
            {
                spawnAreas.spawns[spawnAreaID].currentItems--;
            }

            if (isFarm)
            {
                for (int i = 0; i < farm.count.Length; i++)
                {
                    GameObject drop = PhotonNetwork.Instantiate(pickupItem.name, transform.position, Quaternion.identity, 0);
                    PhotonView photonView1 = PhotonView.Get(drop);
                    photonView1.RPC("SetPickup", RpcTarget.All, farm.farmData.Farms[farm.farmID].items[i].item.itemID, farm.count[i]);
                    farm.count[i] = 0;
                }
            }

            for (int i = 0; i < placeable.drops.Length; i++)
            {
                GameObject drop = PhotonNetwork.Instantiate(pickupItem.name, transform.position, Quaternion.identity, 0);
                PhotonView photonView2 = PhotonView.Get(drop);
                photonView2.RPC("SetPickup", RpcTarget.All, placeable.drops[i].item.itemID, placeable.drops[i].count);
            }

            GridManager gridManager = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>();
            PhotonView photonView3 = PhotonView.Get(gridManager);
            photonView3.RPC("RemoveObject", RpcTarget.All, transform.position);


            PhotonNetwork.Destroy(gameObject);
        }
    }
}
