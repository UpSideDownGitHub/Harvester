using MonoFN.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
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


    public void currentHotbarItems(Dictionary<Item, bool> newHotbar, List<GameObject> UIObjects)
    {
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
        objectIcon.sprite = hotbar[curSelectedItem].icon;
        hotbarUIObjects[curSelectedItem].GetComponent<Image>().color = selectedColor;
    }

    public void SetSelected(int itemToSelect)
    {
        if (hotbarUIObjects.Count == 0)
            return;
        hotbarUIObjects[curSelectedItem].GetComponent<Image>().color = defaultColor;
        curSelectedItem = itemToSelect;
        objectIcon.sprite = hotbar[curSelectedItem].icon;
        hotbarUIObjects[curSelectedItem].GetComponent<Image>().color = selectedColor;
    }

    public void Start()
    {
        curHealth = maxHealth;
        staminaSlider.value = maxStamina;
        currentStamina = maxStamina;
    }

    public void Update()
    {
        // Hotbar
        if (Input.mouseScrollDelta.y > 0)
        {
            var itemToSelect = curSelectedItem + 1 >= hotbarUIObjects.Count ? 0 : curSelectedItem + 1;
            SetSelected(itemToSelect);
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            var itemToSelect = curSelectedItem - 1 < 0 ? hotbarUIObjects.Count - 1 : curSelectedItem - 1;
            SetSelected(itemToSelect);
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
