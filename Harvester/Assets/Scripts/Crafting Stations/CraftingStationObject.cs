using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class CraftingStationObject : MonoBehaviour
{
    [Header("Inventory")]
    public Inventory inventory;
    [Header("World Object")]
    public GameObject UI;
    public Button closeMenuButton;
    public bool inRange;
    private string _interactedPlayer;

    [Header("Station ID")]
    public CraftingStationData stationData;
    public int stationID;

    [Header("UI")]
    public TMP_Text stationName;
    public TMP_Text craftCount;
    public int currentCraftCount;
    public int currentSelectedRecipieID;

    [Header("Crafting Station Recipe")]
    public Transform recipeTransform;
    public GameObject itemRecipePrefab;

    [Header("Crafting Station Info")]
    public TMP_Text itemName;
    public Image icon;
    public Transform itemAmountParent;
    public GameObject itemAmountPrefab;

    [Header("Time To Craft")]
    public bool instantCrafting;
    public Slider craftTimeSlider;
    public Transform itemSpawnPosition;
    public GameObject pickupPrefab;
    public GameObject craftingUI;
    public Image itemIcon;
    public bool crafting;
    public int[] items = new int[2]{ 0, 0 };

    [Header("UI")]
    public GameObject interactionUI;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip craftingFinished;
    public AudioClip click;
    public AudioClip closeMenu;
    public AudioClip openMenu;

    /// <summary>
    /// The Start function initializes and sets up the crafting station menu by spawning recipe items
    /// and setting their information.
    /// </summary>
    void Start()
    {
        inventory = GameObject.FindGameObjectWithTag("Manager").GetComponent<Inventory>();
        interactionUI = GameObject.FindGameObjectWithTag("Manager").GetComponent<MiscManager>().interactUI;

        stationName.text = stationData.stations[stationID].stationName;
        for (int i = 0; i < recipeTransform.childCount; i++)
        {
            Destroy(recipeTransform.GetChild(i).gameObject);
        }

        for (int i = 0; i < stationData.stations[stationID].recipies.Length; i++)
        {
            var spawnedObject = Instantiate(itemRecipePrefab, recipeTransform);
            var recipieItem = spawnedObject.GetComponent<CraftingStationRecipeItem>();
            recipieItem.itemName.text = stationData.stations[stationID].recipies[i].recipeName;
            recipieItem.icon.sprite = stationData.stations[stationID].recipies[i].produces.item.icon;
            recipieItem.ID = i;
            recipieItem.craftingManager = this;
        }
        ItemPressed(0);


        closeMenuButton.onClick.AddListener(() => CloseMenu());
    }

    /// <summary>
    /// The Update function checks if a certain condition is met and then either starts a coroutine or
    /// checks for input to open or close a menu.
    /// </summary>
    public void Update()
    {
        if (items[1] == 1 && !crafting)
        {
            StartCoroutine(AnimateSliderOverTime(stationData.stations[stationID].recipies[currentSelectedRecipieID].time, items[0]));
        }

        if (Input.GetKeyDown(KeyCode.E) && inRange && !crafting)
        {
            if (MenuManager.IsCurrentMenuClose(MenuID.CRAFTING))
            {
                // Close
                audioSource.PlayOneShot(closeMenu);
                CloseMenu();
            }
            else if (MenuManager.CanOpenMenuSet(MenuID.CRAFTING))
            {
                // Open
                audioSource.PlayOneShot(openMenu);
                OpenMenu();
            }
        }
    }

    /// <summary>
    /// The OnTriggerEnter2D function checks if the collision is with a player and if the player is not
    /// already in range, then it sets inRange to true, stores the name of the player, and activates the
    /// interactionUI.
    /// </summary>
    /// <param name="Collider2D">The parameter "Collider2D" is the collider component attached to the
    /// game object that triggered the trigger event. It represents the collider that the player object
    /// collided with.</param>
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !inRange)
        {
            inRange = true;
            _interactedPlayer = collision.name;
            interactionUI.SetActive(true);
        }
    }
    /// <summary>
    /// The OnTriggerExit2D function checks if the collision is with the player and if so, sets inRange
    /// to false, hides the interaction UI and menu UI, and sets the MenuManager's menuOpen variable to
    /// NOTHING.
    /// </summary>
    /// <param name="Collider2D">The Collider2D parameter represents the collider component attached to
    /// the game object that triggered the event. It is used to detect collisions with other objects in
    /// the game world.</param>
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && _interactedPlayer.Equals(collision.name))
        {
            inRange = false;
            _interactedPlayer = null;
            interactionUI.SetActive(false);
            UI.SetActive(false);
            MenuManager.menuOpen = MenuID.NOTHING;
        }
    }

    /// <summary>
    /// The OpenMenu function sets the UI game object to active and calls the ItemPressed function with
    /// an argument of 0.
    /// </summary>
    public void OpenMenu()
    {
        UI.SetActive(true);
        ItemPressed(0);
    }
    /// <summary>
    /// The CloseMenu function sets the menuOpen variable to NOTHING and deactivates the UI.
    /// </summary>
    public void CloseMenu()
    {
        MenuManager.menuOpen = MenuID.NOTHING;
        UI.SetActive(false);
    }

    /// <summary>
    /// The function "ItemPressed" updates the UI elements based on the selected item's ID and count,
    /// and displays the required materials and their quantities for crafting.
    /// </summary>
    /// <param name="ID">The ID parameter represents the ID of the item that was pressed. It is used to
    /// identify the specific recipe that needs to be displayed.</param>
    /// <param name="count">The "count" parameter is an optional parameter with a default value of 1. It
    /// represents the number of items to be crafted. If no value is provided for "count" when calling
    /// the method, it will default to 1.</param>
    public void ItemPressed(int ID, int count = 1)
    {
        audioSource.PlayOneShot(click);

        currentCraftCount = count;
        currentSelectedRecipieID = ID;
        craftCount.text = "x" + currentCraftCount.ToString();
        for (int i = 0; i < itemAmountParent.childCount; i++)
        {
            Destroy(itemAmountParent.GetChild(i).gameObject);
        }

        // Name & Icon
        itemName.text = stationData.stations[stationID].recipies[ID].recipeName;
        icon.sprite = stationData.stations[stationID].recipies[ID].produces.item.icon;

        for (int i = 0; i < stationData.stations[stationID].recipies[ID].materials.Length; i++)
        {
            var spawnedObject = Instantiate(itemAmountPrefab, itemAmountParent);
            var infoItem = spawnedObject.GetComponent<CraftingStationInfoItem>();
            infoItem.icon.sprite = stationData.stations[stationID].recipies[ID].materials[i].item.icon;
            infoItem.itemName.text = stationData.stations[stationID].recipies[ID].materials[i].item.itemName;
            var neededCount = stationData.stations[stationID].recipies[ID].materials[i].count;

            var haveCount = inventory.inventory.ContainsKey(stationData.stations[stationID].recipies[ID].materials[i].item) ? 
                inventory.inventory[stationData.stations[stationID].recipies[ID].materials[i].item] : 0;

            infoItem.itemCount.text = haveCount + "/" + neededCount * count;
        }
    }

    
    /// <summary>
    /// The Craft function is used to craft items by removing the required materials from the inventory
    /// and adding the produced items, either instantly or through a networked RPC call.
    /// </summary>
    /// <returns>
    /// If the condition `currentCraftCount > MaxCraftable() || currentCraftCount <= 0` is true, then
    /// nothing is being returned. The `return` statement will exit the method and no further code will
    /// be executed.
    /// </returns>
    public void Craft()
    {
        if (currentCraftCount > MaxCraftable() || currentCraftCount <= 0)
            return;

        audioSource.PlayOneShot(click);
        // take off the given ammount of materials
        for (int i = 0; i < stationData.stations[stationID].recipies[currentSelectedRecipieID].materials.Length; i++)
        {
            inventory.RemoveItem(stationData.stations[stationID].recipies[currentSelectedRecipieID].materials[i].item,
                stationData.stations[stationID].recipies[currentSelectedRecipieID].materials[i].count * currentCraftCount);
        }

        if (instantCrafting)
        {
            inventory.AddItem(stationData.stations[stationID].recipies[currentSelectedRecipieID].produces.item,
            stationData.stations[stationID].recipies[currentSelectedRecipieID].produces.count * currentCraftCount);
        }
        else
        {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("setItems", RpcTarget.All, currentCraftCount);

        }

        ItemPressed(currentSelectedRecipieID);
    }

    /// <summary>
    /// The function "setItems" sets the value of the "items" array to be an array with two elements,
    /// where the first element is the given "itemCount" and the second element is always 1.
    /// </summary>
    /// <param name="itemCount">The parameter "itemCount" is an integer that represents the number of
    /// items.</param>
    [PunRPC]
    public void setItems(int itemCount)
    {
        items = new int[] { itemCount, 1 };
    }

    /// <summary>
    /// The function "AnimateSliderOverTime" animates a slider over a specified duration and performs
    /// certain actions at each iteration.
    /// </summary>
    /// <param name="seconds">The number of seconds it takes for the slider animation to
    /// complete.</param>
    /// <param name="count">The parameter "count" is an integer that represents the number of times the
    /// animation should run.</param>
    IEnumerator AnimateSliderOverTime(float seconds, int count)
    {
        crafting = true;
        CloseMenu();
        craftingUI.SetActive(true);
        itemIcon.sprite = stationData.stations[stationID].recipies[currentSelectedRecipieID].produces.item.icon;

        print("Should Run: " + count + " Times");
        for (int i = 0; i < count; i++)
        {
            print("RUN: " + i);
            float animationTime = 0f;
            while (animationTime < seconds)
            {
                animationTime += Time.deltaTime;
                float lerpValue = animationTime / seconds;
                craftTimeSlider.value = Mathf.Lerp(0, 1, lerpValue);
                yield return null;
            }

            audioSource.PlayOneShot(craftingFinished);
            GameObject drop = PhotonNetwork.Instantiate(pickupPrefab.name, itemSpawnPosition.position, Quaternion.identity, 0);
            PhotonView photonView = PhotonView.Get(drop);
            photonView.RPC("SetPickup", RpcTarget.All, stationData.stations[stationID].recipies[currentSelectedRecipieID].produces.item.itemID,
                stationData.stations[stationID].recipies[currentSelectedRecipieID].produces.count);
            yield return null;
        }
        craftingUI.SetActive(false);
        crafting = false;
        items = new int[] { 0, 0 };
    }

    /// <summary>
    /// The function "SetToOne" plays a sound effect, sets the currentCraftCount variable to 1, and
    /// calls the ItemPressed function with the currentSelectedRecipieID and currentCraftCount as
    /// arguments.
    /// </summary>
    public void SetToOne()
    {
        audioSource.PlayOneShot(click);
        currentCraftCount = 1;
        ItemPressed(currentSelectedRecipieID, currentCraftCount);
    }
    /// <summary>
    /// The SetMax function plays a click sound, determines the smallest possible craftable count, sets
    /// the current craft count to that value (or 1 if it's less than or equal to 0), and calls the
    /// ItemPressed function with the current selected recipe ID and craft count as arguments.
    /// </summary>
    public void SetMax()
    {
        audioSource.PlayOneShot(click);
        var smallest = MaxCraftable();
        currentCraftCount = smallest <= 0 ? 1 : smallest;
        ItemPressed(currentSelectedRecipieID, currentCraftCount);
    }
    /// <summary>
    /// The function "IncreaseCraft" increases the craft count, plays a click sound, and calls the
    /// "ItemPressed" function with the current selected recipe ID and craft count as parameters.
    /// </summary>
    public void IncreaseCraft()
    {
        audioSource.PlayOneShot(click);
        currentCraftCount++;
        ItemPressed(currentSelectedRecipieID, currentCraftCount);
    }
    /// <summary>
    /// The DecreaseCraft function decreases the currentCraftCount by 1, unless it would result in a
    /// value less than or equal to 0, in which case it sets the value to 1, and then calls the
    /// ItemPressed function with the currentSelectedRecipieID and currentCraftCount as arguments.
    /// </summary>
    public void DecreaseCraft()
    {
        audioSource.PlayOneShot(click);
        currentCraftCount = currentCraftCount - 1 <= 0 ? 1 : currentCraftCount - 1;
        ItemPressed(currentSelectedRecipieID, currentCraftCount);
    }

    /// <summary>
    /// Calculates the maximum number of times the currently selected recipe can be crafted based on available materials.
    /// </summary>
    /// <returns>The maximum craftable quantity.</returns>
    public int MaxCraftable()
    {
        var smallest = 9999;
        for (int i = 0; i < stationData.stations[stationID].recipies[currentSelectedRecipieID].materials.Length; i++)
        {
            var neededCount = stationData.stations[stationID].recipies[currentSelectedRecipieID].materials[i].count;
            var haveCount = inventory.inventory.ContainsKey(stationData.stations[stationID].recipies[currentSelectedRecipieID].materials[i].item) ?
                inventory.inventory[stationData.stations[stationID].recipies[currentSelectedRecipieID].materials[i].item] : 0;

            smallest = haveCount / neededCount < smallest ? haveCount / neededCount : smallest;
        }
        return smallest;
    }
}
