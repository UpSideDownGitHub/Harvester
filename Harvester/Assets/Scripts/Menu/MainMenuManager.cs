using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public GameObject StartMenu;
    public GameObject LobbyMenu;
    

    public void StartPressed()
    {
        StartMenu.SetActive(false);
        LobbyMenu.SetActive(true);
    }
    public void BackPressed()
    {
        StartMenu.SetActive(true);
        LobbyMenu.SetActive(false);
    }
}
