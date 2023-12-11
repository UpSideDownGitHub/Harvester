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

    void Start()
    {
        inventory = GameObject.FindGameObjectWithTag("Manager").GetComponent<Inventory>();
        interactionUI = GameObject.FindGameObjectWithTag("Manager").GetComponent<MiscManager>().interactUI;

        // spawn and set all of the information
        // station name
        stationName.text = stationData.stations[stationID].stationName;

        for (int i = 0; i < recipeTransform.childCount; i++)
        {
            Destroy(recipeTransform.GetChild(i).gameObject);
        }

        // station recipies
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
                CloseMenu();
            }
            else if (MenuManager.CanOpenMenuSet(MenuID.CRAFTING))
            {
                // Open
                OpenMenu();
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !inRange)
        {
            inRange = true;
            _interactedPlayer = collision.name;
            interactionUI.SetActive(true);
        }
    }
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

    public void OpenMenu()
    {
        UI.SetActive(true);
        ItemPressed(0);
    }
    public void CloseMenu()
    {
        MenuManager.menuOpen = MenuID.NOTHING;
        UI.SetActive(false);
    }

    public void ItemPressed(int ID, int count = 1)
    {
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

    
    public void Craft()
    {
        if (currentCraftCount > MaxCraftable() || currentCraftCount <= 0)
            return;

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

    [PunRPC]
    public void setItems(int itemCount)
    {
        items = new int[] { itemCount, 1 };
    }

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

    public void SetToOne()
    {
        currentCraftCount = 1;
        ItemPressed(currentSelectedRecipieID, currentCraftCount);
    }
    public void SetMax()
    {
        var smallest = MaxCraftable();
        currentCraftCount = smallest <= 0 ? 1 : smallest;
        ItemPressed(currentSelectedRecipieID, currentCraftCount);
    }
    public void IncreaseCraft()
    {
        currentCraftCount++;
        ItemPressed(currentSelectedRecipieID, currentCraftCount);
    }
    public void DecreaseCraft()
    {
        currentCraftCount = currentCraftCount - 1 <= 0 ? 1 : currentCraftCount - 1;
        ItemPressed(currentSelectedRecipieID, currentCraftCount);
    }

    /// <summary>
    /// return the maximum craftable of the current selected recipie
    /// </summary>
    /// <returns>smallest - whichever ingredient is lacking the most as this will effect the max amount craftable</returns>
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
