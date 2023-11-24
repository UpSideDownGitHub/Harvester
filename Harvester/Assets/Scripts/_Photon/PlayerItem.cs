using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerItem : MonoBehaviour
{
    public TMP_Text playerName;

    public void SetPlayerName(Photon.Realtime.Player player)
    {
        playerName.text = player.NickName;
    }

    public void ApplyLocalChanges()
    {
        playerName.color = Color.red;
    }
}
