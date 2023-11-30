using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuPlayerID : MonoBehaviour
{
    public int ID;
    public TMP_Text playerName;
    public Image[] heartIcons;
    public LobbyManager lobbyManager;
    public Button button;

    public void Setup(LobbyManager manager)
    {
        lobbyManager = manager;
        button.onClick.AddListener(() => lobbyManager.PlayerSelected(ID));
    }
}
