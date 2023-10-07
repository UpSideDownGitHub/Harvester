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


    public void Start()
    {
        curHealth = maxHealth;
        staminaSlider.value = maxStamina;
        currentStamina = maxStamina;
    }

    public void Update()
    {
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
