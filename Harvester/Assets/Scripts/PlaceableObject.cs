using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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

    public void Start()
    {
        currentHealth = placeable.health;
        healthSlider.minValue = 0;
        healthSlider.maxValue = placeable.health;
        healthSlider.value = healthSlider.maxValue;
    }

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
            DropItems();
            Destroy(gameObject);
        }
    }

    public void DropItems()
    {
        for (int i = 0; i < placeable.drops.Length; i++)
        {
            var pickup = Instantiate(pickupItem, transform.position, Quaternion.identity);
            pickup.GetComponent<Pickup>().SetPickup(placeable.drops[i].item,
                placeable.drops[i].count,
                placeable.drops[i].item.icon);
        }
        GridManager gridManager = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>();
        gridManager.RemoveObject(transform.position);
    }
}
