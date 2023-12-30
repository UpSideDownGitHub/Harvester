using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class Player : MonoBehaviourPunCallbacks
{
    public string playerName;

    [Header("Health")]
    public int maxHealth;
    public int curHealth;
    public Sprite heartFullIcon;
    public Sprite heartEmptyIcon;
    public Image[] healthIcons;
    public bool dead;
    public float deathTime;
    private float _timeOfDeath;

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
    [Range(0, 1)]
    public float maxStaminaToNotIncrease;

    [Header("Placing Consumables")]
    public GridManager gridManager;

    [Header("Boss Spawning")]
    public BossData bosses;
    public Vector3 bossSpawnOffset;
    public BossManager bossManager;

    [Header("Basic Crafting")]
    public CraftingStationObject basicCrafting;

    [Header("Data")]
    public ConsumableObjectData consumableData;
    public ItemData itemData;
    public PlaceableObjectsData placeableData;
    public ToolObjectData toolData;

    private bool isOwner = false;

    [Header("Animations")]
    public PlayerAnimManager anim;
    public Rigidbody2D rb;
    public bool moving;
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

    [Header("Misc Manager")]
    public MiscManager miscManager;
    public NavMeshManager navMeshManager;

    [Header("Audio")]
    public PlayerAudiomanager audioManager;

/// <summary>
/// Called when the local player leaves the room.
/// </summary>
/// <remarks>
/// This method sets player states and communicates with the master client using Photon RPCs.
/// </remarks>
    public override void OnLeftRoom()
    {
        PhotonView miscView = PhotonView.Get(miscManager.gameObject);
        miscView.RPC("SetPlayers", RpcTarget.MasterClient, true, false);
        miscView.RPC("SetPlayers", RpcTarget.MasterClient, false, false);
    }

/// <summary>
/// Sets the player's name.
/// </summary>
/// <param name="givenName">The name to set for the player.</param>
/// <remarks>
/// This method sets the player's name using a Photon RPC.
/// </remarks>
    [PunRPC]
    public void SetName(string givenName)
    {
        this.name = givenName;
    }

/// <summary>
/// Initializes the player's properties and components when the game starts.
/// </summary>
/// <remarks>
/// This method initializes various properties and components of the player such as UI, inventory, crafting, and more.
/// It also loads player data from a save, if available.
/// </remarks>
    public void Start()
    {
        if (photonView.IsMine)
        {
            isOwner = true;
            gridManager = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>();
            var uiManager = GameObject.FindGameObjectWithTag("UI").GetComponent<UIManager>();
            healthIcons = uiManager.healthIcons;
            staminaSlider = uiManager.staminaSlider;
            inventory = GameObject.FindGameObjectWithTag("Manager").GetComponent<Inventory>();
            basicCrafting = GameObject.FindGameObjectWithTag("Manager").GetComponent<CraftingStationObject>();
            bossManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<BossManager>();
            miscManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<MiscManager>();
            navMeshManager = GameObject.FindGameObjectWithTag("NavMesh").GetComponent<NavMeshManager>();
            PhotonView miscView = PhotonView.Get(miscManager.gameObject);
            miscView.RPC("SetPlayers", RpcTarget.MasterClient, true, true);
            miscView.RPC("SetPlayers", RpcTarget.MasterClient, false, true);
            curHealth = maxHealth;
            staminaSlider.value = maxStamina;
            currentStamina = maxStamina;
            inventory.player = this;
            spawnPoint = GameObject.FindGameObjectWithTag("Spawn").transform;

            // load all info from the save
            var save = SaveManager.instance.LoadPlayerSaveData();
            var pickedData = SaveManager.instance.LoadGeneralSaveData();
            playerName = save.players[pickedData.playerID].playerName;
            inventory.SetInventory(save.players[pickedData.playerID].inventory, save.players[pickedData.playerID].hotbar);
        }
        else
            isOwner = false;
    }

/// <summary>
/// Plays the appropriate animation based on the player's current state and actions.
/// </summary>
/// <remarks>
/// This method manages the player's animations for actions like walking, attacking, mining, and others.
/// It uses an animation manager to handle the animation states.
/// </remarks>
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
                        anim.ChangeAnimationState(PlayerAnimManager.Walk_Up);
                    break;
                case 1: // RIGHT
                        anim.ChangeAnimationState(PlayerAnimManager.Walk_Right);
                    break;
                case 2: // DOWN
                        anim.ChangeAnimationState(PlayerAnimManager.Walk_Down);
                    break;
                case 3: // LEFT
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
                        anim.ChangeAnimationState(PlayerAnimManager.Idle_Up);
                    break;
                case 1: // RIGHT
                        anim.ChangeAnimationState(PlayerAnimManager.Idle_Right);
                    break;
                case 2: // DOWN
                        anim.ChangeAnimationState(PlayerAnimManager.Idle);
                    break;
                case 3: // LEFT
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
/// <summary>
/// Determines the player's movement direction based on the provided movement vector.
/// </summary>
/// <param name="movement">The movement vector indicating the direction of movement.</param>
/// <returns>An integer representing the movement direction: 0 (Down), 1 (Right), 2 (Up), 3 (Left).</returns>
/// <remarks>
/// This function compares the magnitudes of the movement vector components and returns the corresponding direction.
/// </remarks>
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

/// <summary>
/// Updates the player's hotbar with the given dictionary of items, UI objects, and an optional flag to keep the current selection.
/// </summary>
/// <param name="newHotbar">The dictionary representing the new hotbar items and their availability status.</param>
/// <param name="UIObjects">The list of UI objects corresponding to the hotbar items.</param>
/// <param name="keepSelected">Flag indicating whether to keep the currently selected item.</param>
/// <remarks>
/// This function clears the current hotbar, populates it with available items from the provided dictionary,
/// and updates the UI representation of the hotbar. It also allows keeping the current selected item if specified.
/// </remarks>
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
            PlayAnimation();
            return;
        }
        if (keepSelected)
            curSelectedItem = previousSelected >= hotbar.Count ? hotbar.Count - 1 : previousSelected;
        objectIcon.sprite = hotbar[curSelectedItem].icon;
        hotbarUIObjects[curSelectedItem].GetComponent<Image>().color = selectedColor;
    }
/// <summary>
/// Sets the selected item in the hotbar and updates the UI representation.
/// </summary>
/// <param name="itemToSelect">The index of the item to select in the hotbar.</param>
/// <remarks>
/// This function changes the selected item in the hotbar and updates the UI representation accordingly.
/// </remarks>
    public void SetSelected(int itemToSelect)
    {
        if (hotbarUIObjects.Count == 0)
        {
            return;
        }
        hotbarUIObjects[curSelectedItem].GetComponent<Image>().color = defaultColor;
        curSelectedItem = itemToSelect;
        objectIcon.sprite = hotbar[curSelectedItem].icon;
        hotbarUIObjects[curSelectedItem].GetComponent<Image>().color = selectedColor;
    }
/// <summary>
/// Updates the player's state and performs various actions such as handling menus, hotbar selection, and item usage.
/// </summary>
/// <remarks>
/// This function checks the player's state, handles menu interactions, updates the hotbar selection,
/// and triggers item usage based on player input. It also manages stamina and health mechanics.
/// </remarks>
    public void Update()
    {
        if (!isOwner)
            return;
        if (dead)
        {
            if (Time.time > deathTime + _timeOfDeath && !bossManager.isBossAlive())
            {
                transform.position = spawnPoint.position;
                dead = false;
                // this is not meant to be used like this but there seems to be an issue with the death animation playing forever when the player dies
                hit = false;
                die = false;
                curHealth = maxHealth;
                currentStamina = maxStamina;
                PhotonView miscView = PhotonView.Get(miscManager.gameObject);
                miscView.RPC("SetPlayers", RpcTarget.MasterClient, true, true);
                UpdateHealthUI();
                print(curHealth + " Current Health");
                miscManager.deathUI.SetActive(false);
            }
            return;
        }

        PlayAnimation();

        // Basic Crafting Station
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (MenuManager.IsCurrentMenuClose(MenuID.INSTANTCRAFTING))
            {
                audioManager.PlayCloseMenu();    
                basicCrafting.CloseMenu(); 
            }
            else if (MenuManager.CanOpenMenuSet(MenuID.INSTANTCRAFTING))
            {
                audioManager.PlayOpenMenu();
                basicCrafting.OpenMenu(); 
            }
        }



        if (MenuManager.IsMenuOpen())
        {   
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
        }

        // ITEM USAGE
        // used button pressed when not in the inventory
        if(Input.GetMouseButtonDown(0) && MenuManager.IsMenuOpen() && 
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
                photonView.RPC("DecreaseHealth", RpcTarget.All);
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

/// <summary>
/// Uses the currently selected item from the player's hotbar, performing various actions based on the item type.
/// </summary>
/// <remarks>
/// If the selected item is consumable, it checks the player's stamina level and increases it if below a certain threshold.
/// If the selected item is placeable, it attempts to place the object on the grid and removes the item from the inventory if successful.
/// If the selected item is a tool (e.g., sword, pickaxe, axe), it triggers corresponding actions (e.g., attacking, mining) and handles interactions with enemies or objects.
/// If the selected item is a boss spawn item, it checks if a boss is already alive before spawning a new one.
/// </remarks>
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
                audioManager.PlayEat();

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

            if (toolData.Tools[itemType.toolID].type == ToolType.SWORD)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

                if (hit.collider != null)
                {
                    Debug.Log("CLICKED " + hit.collider.name);
                    if (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Boss"))
                    {
                        // deal damage to the enemy
                        PhotonView photonView = PhotonView.Get(hit.collider.gameObject);
                        photonView.RPC("TakeDamage", RpcTarget.All, toolData.Tools[itemType.toolID].toolDamage[itemType.toolLevel]);

                    }
                }
            }
            else
            {
                var clickedObject = gridManager.ObjectClicked(Input.mousePosition);
                print("Clicked Object: " + clickedObject);
                if (clickedObject != null)
                {
                    print("Object Clicked");
                    var placeable = clickedObject.GetComponent<PlaceableObject>();
                    print("Break Type: " + placeable.placeable.breakType);
                    print("Tool Type: " + toolData.Tools[itemType.toolID].type);
                    if (placeable.placeable.breakType == toolData.Tools[itemType.toolID].type)
                    {
                        PhotonView photonView = PhotonView.Get(placeable);
                        photonView.RPC("TakeDamage", RpcTarget.All, toolData.Tools[itemType.toolID].toolDamage[itemType.toolLevel]);
                    }
                }
            }
        }
        else if (itemType.boss) // Boss Spawn
        {
            print("Boss");
            // need to add a check here to check if a boss has already been spawned and if there is then dont allow another to be spawned
            if (!bossManager.isBossAlive())
                SpawnBoss(itemType.bossID, Input.mousePosition + bossSpawnOffset);
        }    
        else
        {
            // materials cannot be used and are only used in crafting recipies
            print("This Item Is A Material");
        }
    }

/// <summary>
/// Spawns a boss enemy based on the provided boss ID at the specified world position.
/// </summary>
/// <param name="bossID">The identifier of the boss enemy.</param>
/// <param name="bossSpawnPosition">The screen position where the boss should spawn.</param>
/// <remarks>
/// This function converts the screen position to world position, finds the closest NavMesh position,
/// and instantiates a boss enemy using PhotonNetwork.
/// </remarks>
    public void SpawnBoss(int bossID, Vector3 bossSpawnPosition)
    {
        var worldPosision = Camera.main.ScreenToWorldPoint(bossSpawnPosition);
        var spawnPos = navMeshManager.GetClosestNavMeshPosition(worldPosision);
        PhotonNetwork.Instantiate("Enemies/" + bosses.bosses[bossID].bossObject.name, spawnPos, Quaternion.identity, 0);
    }

/// <summary>
/// Increases the player's stamina by the specified amount.
/// </summary>
/// <param name="amount">The amount by which to increase the stamina.</param>
/// <remarks>
/// This function increments the current stamina, ensuring it does not exceed the maximum stamina.
/// </remarks>
    public void IncreaseStamina(float amount)
    {
        currentStamina = currentStamina + amount > maxStamina ? maxStamina : currentStamina + amount;
    }
/// <summary>
/// Decreases the player's stamina by the specified amount.
/// </summary>
/// <param name="amount">The amount by which to decrease the stamina.</param>
/// <remarks>
/// This function decrements the current stamina, ensuring it does not go below zero.
/// </remarks>
    public void DecreaseStamina(float amount)
    {
        currentStamina = currentStamina - amount < 0 ? 0 : currentStamina - amount;
    }

/// <summary>
/// Increases the player's health by one unit.
/// </summary>
/// <remarks>
/// This function increments the current health by one, ensuring it does not exceed the maximum health.
/// It also updates the health UI after the increase.
/// </remarks>
    [PunRPC]
    public void IncreaseHealth()
    {
        curHealth = curHealth + 1 >= maxHealth ? maxHealth : curHealth + 1;
        UpdateHealthUI();
    }
/// <summary>
/// Decreases the player's health by one unit, plays a lose heart sound, and updates the health UI.
/// </summary>
/// <remarks>
/// If the player is already dead, the function returns without further processing.
/// If the health drops to zero, the player is considered dead, and various game-related actions are performed.
/// </remarks>
    [PunRPC]
    public void DecreaseHealth()
    {
        if (dead)
            return;

        audioManager.PlayLoseHeart();
        curHealth = curHealth - 1 < 0 ? 0 : curHealth - 1;
        UpdateHealthUI();

        if (curHealth - 1 < 0)
        {
            // Kill the player and end the game
            die = true;
            PlayAnimation();
            dead = true;
            _timeOfDeath = Time.time;

            if (photonView.IsMine)
            {
                PhotonView miscView = PhotonView.Get(miscManager.gameObject);
                miscView.RPC("SetPlayers", RpcTarget.MasterClient, true, false);
            }
            if (miscManager)
                miscManager.deathUI.SetActive(true);
            return;
        }

        hit = true;
        PlayAnimation();
    }
/// <summary>
/// Updates the player's health UI based on the current health.
/// </summary>
/// <remarks>
/// This function iterates through health icons, setting them to full or empty based on the player's current health.
/// </remarks>
    public void UpdateHealthUI()
    {
        if (!isOwner)
            return;
        for (int i = 0; i < maxHealth; i++)
        {
            if (i < curHealth)
                healthIcons[i].sprite = heartFullIcon;
            else
                healthIcons[i].sprite = heartEmptyIcon;
        }
    }
/// <summary>
/// Adds an item to the player's inventory based on the provided item ID and count.
/// </summary>
/// <param name="itemID">The identifier of the item to add.</param>
/// <param name="count">The quantity of the item to add.</param>
/// <remarks>
/// This function plays a pickup sound and adds the specified item to the player's inventory if the inventory is available.
/// </remarks>
    [PunRPC]
    public void AddItemToInventory(int itemID, int count)
    {
        audioManager.PlayPickup();
        if (inventory)
            inventory.AddItem(itemData.items[itemID], count);
    }
}
