using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuMapID : MonoBehaviour
{
    public int ID;
    public TMP_Text mapName;
    public Image mapIcon;
    public LobbyManager lobbyManager;
    public Button button;

    public void Setup(LobbyManager manager)
    {
        lobbyManager = manager;
        button.onClick.AddListener(() => lobbyManager.MapSelected(ID));
    }
}
