using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    public string playerName;

    [Header("Health")]
    public int maxHealth;
    [SyncVar(OnChange = "UpdateHealth")]public int curHealth;
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

    [Header("Animations")]
    public PlayerAnimManager anim;
    public Rigidbody2D rb;
    public bool moving;
    public bool holdingObject;
    public bool attack;
    public bool mine;
    public bool axe;
    public bool die;
    public bool hit;
    public Vector2 previousVelocity;
    public float movingMagnitudeThreshold = 0.1f;
    private string currentAnimName;

    private List<string> attackSimilar = new List<string> { PlayerAnimManager.Attack_Right,
                                                      PlayerAnimManager.Attack_Left,
                                                      PlayerAnimManager.Attack_Up,
                                                      PlayerAnimManager.Attack_Down};
    private List<string> mineSimilar = new List<string> { PlayerAnimManager.Mine_Right,
                                                      PlayerAnimManager.Mine_Left,
                                                      PlayerAnimManager.Mine_Up,
                                                      PlayerAnimManager.Mine_Down};
    private List<string> axeSimilar = new List<string> { PlayerAnimManager.Axe_Right,
                                                      PlayerAnimManager.Axe_Left,
                                                      PlayerAnimManager.Axe_Up,
                                                      PlayerAnimManager.Axe_Down};

    public Transform spawnPoint;

    [Header("Save Data")]
    public PickedData pickedData;


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
            spawnPoint = GameObject.FindGameObjectWithTag("Spawn").transform;

            // load all info from the save
            var save = SaveManager.instance.LoadPlayerSaveData();
            playerName = save.players[pickedData.playerID].playerName;
            inventory.SetInventory(save.players[pickedData.playerID].inventory, save.players[pickedData.playerID].hotbar);
        }
    }

    public void PlayAnimation()
    {
        var velo = rb.velocity;
        moving = velo.magnitude > movingMagnitudeThreshold ? true : false;
        int movingDirection = MovingDirection(velo);
        if (!moving)
            movingDirection = MovingDirection(previousVelocity);

        if (die)
        {
            anim.ChangeAnimationState(PlayerAnimManager.Die);
            if (anim.finished(PlayerAnimManager.Die))
                die = false;
        }
        else if (hit)
        {
            anim.ChangeAnimationState(PlayerAnimManager.Pickup);
            if (anim.finished(PlayerAnimManager.Pickup))
                hit = false;
        }
        else if (attack)
        {
            bool changed;
            switch (movingDirection)
            {
                case 0: // UP
                    changed = anim.ChangeAnimationState(PlayerAnimManager.Attack_Up, attackSimilar);
                    currentAnimName = changed ? PlayerAnimManager.Attack_Up : currentAnimName;
                    break;
                case 1: // RIGHT
                    changed = anim.ChangeAnimationState(PlayerAnimManager.Attack_Right, attackSimilar);
                    currentAnimName = changed ? PlayerAnimManager.Attack_Right : currentAnimName;
                    break;
                case 2: // DOWN
                    changed = anim.ChangeAnimationState(PlayerAnimManager.Attack_Down, attackSimilar);
                    currentAnimName = changed ? PlayerAnimManager.Attack_Down : currentAnimName;
                    break;
                case 3: // LEFT
                    changed = anim.ChangeAnimationState(PlayerAnimManager.Attack_Left, attackSimilar);
                    currentAnimName = changed ? PlayerAnimManager.Attack_Left : currentAnimName;
                    break;
                default:
                    print("ERROR: Should Not Be Here");
                    break;
            }
            if (anim.finished(currentAnimName))
                attack = false;
        }
        else if (mine)
        {
            bool changed;
            switch (movingDirection)
            {
                case 0: // UP
                    changed = anim.ChangeAnimationState(PlayerAnimManager.Mine_Up, mineSimilar);
                    currentAnimName = changed ? PlayerAnimManager.Mine_Up : currentAnimName;
                    break;
                case 1: // RIGHT
                    changed = anim.ChangeAnimationState(PlayerAnimManager.Mine_Right, mineSimilar);
                    currentAnimName = changed ? PlayerAnimManager.Mine_Right : currentAnimName;
                    break;
                case 2: // DOWN
                    changed = anim.ChangeAnimationState(PlayerAnimManager.Mine_Down, mineSimilar);
                    currentAnimName = changed ? PlayerAnimManager.Mine_Down : currentAnimName;
                    break;
                case 3: // LEFT
                    changed = anim.ChangeAnimationState(PlayerAnimManager.Mine_Left, mineSimilar);
                    currentAnimName = changed ? PlayerAnimManager.Mine_Left : currentAnimName;
                    break;
                default:
                    print("ERROR: Should Not Be Here");
                    break;
            }
            if (anim.finished(currentAnimName))
                mine = false;
        }
        else if (axe)
        {
            bool changed;
            switch (movingDirection)
            {
                case 0: // UP
                    changed = anim.ChangeAnimationState(PlayerAnimManager.Axe_Up, axeSimilar);
                    currentAnimName = changed ? PlayerAnimManager.Axe_Up : currentAnimName;
                    break;
                case 1:
                    changed = anim.ChangeAnimationState(PlayerAnimManager.Axe_Right, axeSimilar);
                    currentAnimName = changed ? PlayerAnimManager.Axe_Right : currentAnimName;
                    break;
                case 2: // DOWN
                    changed = anim.ChangeAnimationState(PlayerAnimManager.Axe_Down, axeSimilar);
                    currentAnimName = changed ? PlayerAnimManager.Axe_Down : currentAnimName;
                    break;
                case 3: // LEFT
                    changed = anim.ChangeAnimationState(PlayerAnimManager.Axe_Left, axeSimilar);
                    currentAnimName = changed ? PlayerAnimManager.Axe_Left : currentAnimName;
                    break;
                default:
                    print("ERROR: Should Not Be Here");
                    break;
            }
            if (anim.finished(currentAnimName))
                axe = false;
        }
        else if (moving)
        {
            switch (movingDirection)
            {
                case 0: // UP
                    if (holdingObject)
                        anim.ChangeAnimationState(PlayerAnimManager.CarryWalk_Up);
                    else
                        anim.ChangeAnimationState(PlayerAnimManager.Walk_Up);
                    break;
                case 1: // RIGHT
                    if (holdingObject)
                        anim.ChangeAnimationState(PlayerAnimManager.CarryWalk_Right);
                    else
                        anim.ChangeAnimationState(PlayerAnimManager.Walk_Right);
                    break;
                case 2: // DOWN
                    if (holdingObject)
                        anim.ChangeAnimationState(PlayerAnimManager.CarryWalk_Down);
                    else
                        anim.ChangeAnimationState(PlayerAnimManager.Walk_Down);
                    break;
                case 3: // LEFT
                    if (holdingObject)
                        anim.ChangeAnimationState(PlayerAnimManager.CarryWalk_Left);
                    else
                        anim.ChangeAnimationState(PlayerAnimManager.Walk_Left);
                    break;
                default:
                    print("ERROR: Should Not Be Here");
                    break;
            }
        }
        else
        {
            switch (movingDirection)
            {
                case 0: // UP
                    if (holdingObject)
                        anim.ChangeAnimationState(PlayerAnimManager.CarryIdle_Up);
                    else
                        anim.ChangeAnimationState(PlayerAnimManager.Idle_Up);
                    break;
                case 1: // RIGHT
                    if (holdingObject)
                        anim.ChangeAnimationState(PlayerAnimManager.CarryIdle_Right);
                    else
                        anim.ChangeAnimationState(PlayerAnimManager.Idle_Right);
                    break;
                case 2: // DOWN
                    if (holdingObject)
                        anim.ChangeAnimationState(PlayerAnimManager.CarryIdle_Down);
                    else
                        anim.ChangeAnimationState(PlayerAnimManager.Idle);
                    break;
                case 3: // LEFT
                    if (holdingObject)
                        anim.ChangeAnimationState(PlayerAnimManager.CarryIdle_Left);
                    else
                        anim.ChangeAnimationState(PlayerAnimManager.Idle_Left);
                    break;
                default:
                    print("ERROR: Should Not Be Here");
                    break;
            }
        }
        if (moving)
            previousVelocity = velo;
    }
    public int MovingDirection(Vector2 movement)
    {
        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y)) // moving in X
        {
            if (movement.x < 0) // LEFT
                return 3;
            else // RIGHT
                return 1;
        }
        else // moving in y
        {
            if (movement.y < 0) // UP
                return 2;
            else // DOWN
                return 0;
        }
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
            holdingObject = false;
            PlayAnimation();
            return;
        }
        holdingObject = true;
        PlayAnimation();
        if (keepSelected)
            curSelectedItem = previousSelected >= hotbar.Count ? hotbar.Count - 1 : previousSelected;
        objectIcon.sprite = hotbar[curSelectedItem].icon;
        playerHolding.SetHolding(hotbar[curSelectedItem].itemID);
        hotbarUIObjects[curSelectedItem].GetComponent<Image>().color = selectedColor;
    }

    public void SetSelected(int itemToSelect)
    {
        if (hotbarUIObjects.Count == 0)
        {
            holdingObject = false;
            PlayAnimation();
            return;
        }
        holdingObject = true;
        PlayAnimation();
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

        PlayAnimation();

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
            if (toolData.Tools[itemType.toolID].type == ToolType.SWORD)
            {
                attack = true;
                PlayAnimation();
            }
            else if (toolData.Tools[itemType.toolID].type == ToolType.PICKAXE)
            {
                mine = true;
                PlayAnimation();
            }
            else if (toolData.Tools[itemType.toolID].type == ToolType.AXE)
            {
                axe = true;
                PlayAnimation();
            }


            var clickedObject = gridManager.ObjectClicked(Input.mousePosition);
            if (clickedObject != null)
            {
                print("Object Clicked");
                var placeable = clickedObject.GetComponent<PlaceableObject>();
                //print("Break Type: " + placeable.placeable.breakType);
                //print("Tool Type: " + toolData.Tools[itemType.toolID].type);
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

    public void UpdateHealth(int oldValue, int newValue, bool asServer)
    {
        if (asServer)
            return;
        UpdateHealthUI();
        if (curHealth - 1 < 0)
        {
            die = true;
            PlayAnimation();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void IncreaseHealth()
    {
        curHealth = curHealth + 1 >= maxHealth ? maxHealth : curHealth + 1;
        UpdateHealthUI();
    }
    [ServerRpc(RequireOwnership = false)]
    public void DecreaseHealth()
    {
        curHealth = curHealth - 1 < 0 ? 0 : curHealth - 1;
        UpdateHealthUI();

        hit = true;
        PlayAnimation();

        if (curHealth - 1 < 0)
        {
            // Kill the player and end the game
            die = true;
            PlayAnimation();
            curHealth = maxHealth;
            transform.position = spawnPoint.position;
        }
    }
    public void UpdateHealthUI()
    {
        if (!IsOwner)
            return;
        for (int i = 0; i < maxHealth; i++)
        {
            if (i < curHealth)
                healthIcons[i].sprite = heartFullIcon;
            else
                healthIcons[i].sprite = heartEmptyIcon;
        }
    }
}
