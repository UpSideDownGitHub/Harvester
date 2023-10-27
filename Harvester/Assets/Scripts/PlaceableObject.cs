using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.UI;

public class PlaceableObject : NetworkBehaviour
{
    [Header("Personal Data")]
    public Placeable placeable;
    [SyncVar(OnChange = "syncHealth")]public float currentHealth;
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

    public void Start()
    {
        currentHealth = placeable.health;
        healthSlider.minValue = 0;
        healthSlider.maxValue = placeable.health;
        healthSlider.value = healthSlider.maxValue;
    }

    public void syncHealth(float oldValue, float newValue, bool asServer)
    {
        if (asServer)
            return;

        if (!UICanvas.activeInHierarchy && currentHealth < placeable.health)
            UICanvas.SetActive(true);

        currentHealth = newValue;
        healthSlider.value = currentHealth;

        if (currentHealth == 0)
            ServerManager.Despawn(gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamage(float damage)
    {
        if (isCraftingStation)
        {
            if (craftingStation.crafting)
                return;
        }
        else if (isFarm)
        {
            farm.DropAllItems();
        }

        if (!UICanvas.activeInHierarchy)
            UICanvas.SetActive(true);

        currentHealth = currentHealth - damage < 0 ? 0 : currentHealth - damage;
        healthSlider.value = currentHealth;
        if (currentHealth == 0)
        {
            for (int i = 0; i < placeable.drops.Length; i++)
            {
                GameObject drop = Instantiate(pickupItem, transform.position, Quaternion.identity);
                ServerManager.Spawn(drop);
                drop.GetComponent<Pickup>().info = new int[2] { placeable.drops[i].item.itemID, placeable.drops[i].count };
            }

            GridManager gridManager = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>();
            gridManager.RemoveObject(transform.position);

            ServerManager.Despawn(gameObject);
        }
    }
}
