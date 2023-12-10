using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;

public class Player : MonoBehaviour
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
    [Range(0,1)]
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

    [Header("Photon Things")]
    PhotonView photonView;

    public void Start()
    {
        photonView = PhotonView.Get(this);
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
            miscManager.currentAlivePlayers++;
            miscManager.currentPlayers++;
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
            PlayAnimation();
            return;
        }
        if (keepSelected)
            curSelectedItem = previousSelected >= hotbar.Count ? hotbar.Count - 1 : previousSelected;
        objectIcon.sprite = hotbar[curSelectedItem].icon;
        hotbarUIObjects[curSelectedItem].GetComponent<Image>().color = selectedColor;
    }

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
                miscManager.currentAlivePlayers++;
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
                basicCrafting.CloseMenu();
            else if (MenuManager.CanOpenMenuSet(MenuID.INSTANTCRAFTING))
                basicCrafting.OpenMenu();
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

    public void SpawnBoss(int bossID, Vector3 bossSpawnPosition)
    {
        var worldPosision = Camera.main.ScreenToWorldPoint(bossSpawnPosition);
        var spawnPos = navMeshManager.GetClosestNavMeshPosition(worldPosision);
        PhotonNetwork.Instantiate("Enemies/" + bosses.bosses[bossID].bossObject.name, spawnPos, Quaternion.identity, 0);
    }

    public void IncreaseStamina(float amount)
    {
        currentStamina = currentStamina + amount > maxStamina ? maxStamina : currentStamina + amount;
    }
    public void DecreaseStamina(float amount)
    {
        currentStamina = currentStamina - amount < 0 ? 0 : currentStamina - amount;
    }

    [PunRPC]
    public void IncreaseHealth()
    {
        curHealth = curHealth + 1 >= maxHealth ? maxHealth : curHealth + 1;
        UpdateHealthUI();
    }
    [PunRPC]
    public void DecreaseHealth()
    {
        if (dead)
            return;
        curHealth = curHealth - 1 < 0 ? 0 : curHealth - 1;
        UpdateHealthUI();

        if (curHealth - 1 < 0)
        {
            // Kill the player and end the game
            die = true;
            PlayAnimation();
            dead = true;
            _timeOfDeath = Time.time;
            miscManager.currentAlivePlayers--;
            if (miscManager)
                miscManager.deathUI.SetActive(true);
            return;
        }

        hit = true;
        PlayAnimation();
    }
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

    [PunRPC]
    public void AddItemToInventory(int itemID, int count)
    {
        if (inventory)
            inventory.AddItem(itemData.items[itemID], count);
    }
}
