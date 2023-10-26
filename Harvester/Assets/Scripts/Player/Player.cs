using FishNet.Object;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    [Header("Health")]
    public int maxHealth;
    public int curHealth;
    public Sprite heartFullIcon;
    public Sprite heartEmptyIcon;
    public Image[] healthIcons;

    [Header("Stamina")]
    public float maxStamina;
    public float currentStamina;
    public Slider staminaSlider;
    public float baseDecreaseRate;
    public bool stainaRanOut;
    public float decreaseHealthTime;
    private float _timeSinceLastHealthDecrease;

    [Header("Hotbar/ItemUsage")]
    public Inventory inventory;
    public List<Item> hotbar = new();
    public List<GameObject> hotbarUIObjects = new();
    public int curSelectedItem;
    public SpriteRenderer objectIcon;

    public Color selectedColor;
    public Color defaultColor;

    [Header("Eating Consumables")]
    [Range(0,1)]
    public float maxStaminaToNotIncrease;

    [Header("Placing Consumables")]
    public GridManager gridManager;

    [Header("Basic Crafting")]
    public CraftingStationObject basicCrafting;

    [Header("Data")]
    public ConsumableObjectData consumableData;
    public ItemData itemData;
    public PlaceableObjectsData placeableData;
    public ToolObjectData toolData;

    [Header("Holding Sync")]
    public PlayerHolding playerHolding;
    private bool isOwner = false;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            isOwner = true;
            gridManager = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>();
            var uiManager = GameObject.FindGameObjectWithTag("UI").GetComponent<UIManager>();
            healthIcons = uiManager.healthIcons;
            staminaSlider = uiManager.staminaSlider;
            inventory = GameObject.FindGameObjectWithTag("Manager").GetComponent<Inventory>();
            basicCrafting = GameObject.FindGameObjectWithTag("Manager").GetComponent<CraftingStationObject>();
            curHealth = maxHealth;
            staminaSlider.value = maxStamina;
            currentStamina = maxStamina;
            inventory.player = this;
        }
        else
            gameObject.GetComponent<Player>().enabled = false;
    }

    public void currentHotbarItems(Dictionary<Item, bool> newHotbar, List<GameObject> UIObjects, bool keepSelected = false)
    {
        var previousSelected = curSelectedItem;
        curSelectedItem = 0;
        hotbar.Clear();
        hotbarUIObjects = UIObjects;
        foreach (KeyValuePair<Item, bool> item in newHotbar)
        {
            if (item.Value)
                hotbar.Add(item.Key);
        }
        if (hotbar.Count == 0)
        {
            objectIcon.sprite = null;
            return;
        }
        if (keepSelected)
            curSelectedItem = previousSelected >= hotbar.Count ? hotbar.Count - 1 : previousSelected;
        objectIcon.sprite = hotbar[curSelectedItem].icon;
        playerHolding.SetHolding(hotbar[curSelectedItem].itemID);
        hotbarUIObjects[curSelectedItem].GetComponent<Image>().color = selectedColor;
    }

    public void SetSelected(int itemToSelect)
    {
        if (hotbarUIObjects.Count == 0)

            return;
        hotbarUIObjects[curSelectedItem].GetComponent<Image>().color = defaultColor;
        curSelectedItem = itemToSelect;
        objectIcon.sprite = hotbar[curSelectedItem].icon;
        playerHolding.SetHolding(hotbar[curSelectedItem].itemID);
        hotbarUIObjects[curSelectedItem].GetComponent<Image>().color = selectedColor;
    }

    public void Update()
    {
        if (!isOwner)
            return;

        // Basic Crafting Station
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (basicCrafting.UI.activeInHierarchy)
                basicCrafting.CloseMenu();
            else
            {
                if (inventory.isInventoryOpen())
                    return;

                basicCrafting.OpenMenu(); 
            }
        }

        // HOTBAR
        if (Input.mouseScrollDelta.y < 0)
        {
            var itemToSelect = curSelectedItem + 1 >= hotbarUIObjects.Count ? 0 : curSelectedItem + 1;
            SetSelected(itemToSelect);
        }
        else if (Input.mouseScrollDelta.y > 0)
        {
            var itemToSelect = curSelectedItem - 1 < 0 ? hotbarUIObjects.Count - 1 : curSelectedItem - 1;
            SetSelected(itemToSelect);
        }
        
        // ITEM USAGE
        // used button pressed when not in the inventory
        if(Input.GetMouseButtonDown(0) && !inventory.isInventoryOpen() && 
            !EventSystem.current.IsPointerOverGameObject())
        {
            UseItem();
        }

        // Stamina & Health
        if (stainaRanOut)
        {
            if (currentStamina > staminaSlider.minValue)
                stainaRanOut = false;
            if (Time.time > decreaseHealthTime + _timeSinceLastHealthDecrease)
            {
                _timeSinceLastHealthDecrease = Time.time;
                DecreaseHealth();
            }
        }
        else
        {
            currentStamina -= baseDecreaseRate * Time.deltaTime;
            staminaSlider.value = currentStamina;

            if (staminaSlider.value <= staminaSlider.minValue)
            {
                _timeSinceLastHealthDecrease = Time.time;
                stainaRanOut = true;
            }
        }
    }

    public void UseItem()
    {
        if (hotbar.Count == 0)
            return;
        Item itemType = hotbar[curSelectedItem];
        if (itemType.consumable) // consumable
        {
            print("Consumable");
            if (staminaSlider.value / staminaSlider.maxValue < maxStaminaToNotIncrease)
            {
                var staminaIncreaseAmount = consumableData.consumables[itemType.consumableObjectID].staminaIncrease;
                IncreaseStamina(staminaIncreaseAmount);
                inventory.RemoveItem(itemType, 1, true);
            }
        }
        else if (itemType.placeable) // placeable
        {
            print("Placeable");
            var placed = gridManager.placeObject(itemType.placeableObjectID, Input.mousePosition);
            if (placed)
                inventory.RemoveItem(itemType, 1, true);

        }
        else if (itemType.tool) // tool
        {
            print("Tool");
            var clickedObject = gridManager.ObjectClicked(Input.mousePosition);
            if (clickedObject != null)
            {
                var placeable = clickedObject.GetComponent<PlaceableObject>();
                if (placeable.placeable.breakType == toolData.Tools[itemType.toolID].type)
                {
                    placeable.TakeDamage(toolData.Tools[itemType.toolID].toolDamage[toolData.Tools[itemType.toolID].level]);
                }
            }
        }
        else
        {
            // materials cannot be used and are only used in crafting recipies
            print("This Item Is A Material");
        }
    }

    public void IncreaseStamina(float amount)
    {
        currentStamina += amount;
    }
    public void DecreaseStamina(float amount)
    {
        currentStamina -= amount;
    }

    public void IncreaseHealth()
    {
        curHealth = curHealth + 1 >= maxHealth ? maxHealth : curHealth + 1;
        UpdateHealthUI();
    }
    public void DecreaseHealth()
    {
        curHealth = curHealth - 1 < 0 ? 0 : curHealth - 1;
        UpdateHealthUI();
        
        if (curHealth - 1 < 0)
        {
            // Kill the player and end the game
            print("Player Dead");
        }
    }
    public void UpdateHealthUI()
    {
        for (int i = 0; i < maxHealth; i++)
        {
            if (i < curHealth)
                healthIcons[i].sprite = heartFullIcon;
            else
                healthIcons[i].sprite = heartEmptyIcon;
        }
    }
}
