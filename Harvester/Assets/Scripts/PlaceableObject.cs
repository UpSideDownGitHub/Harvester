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

        if (!UICanvas.activeInHierarchy)
            UICanvas.SetActive(true);

        currentHealth = currentHealth - damage < 0 ? 0 : currentHealth - damage;
        healthSlider.value = currentHealth;
        if (currentHealth == 0)
        {
            for (int i = 0; i < placeable.drops.Length; i++)
            {
                GameObject drop = Instantiate(placeable.drops[i].item, transform.position, Quaternion.identity);
                drop.GetComponent<Pickup>().count = placeable.drops[i].count;
                ServerManager.Spawn(drop);
            }
            ServerManager.Despawn(gameObject);
            GridManager gridManager = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>();
            gridManager.RemoveObject(transform.position);
            ServerManager.Despawn(gameObject);
        }
    }
}
