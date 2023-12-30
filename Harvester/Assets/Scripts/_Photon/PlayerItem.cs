using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/* The PlayerItem class is a MonoBehaviour that displays the player's name and allows for applying
local changes to the player's name. */
public class PlayerItem : MonoBehaviour
{
    public TMP_Text playerName;

    /// <summary>
    /// The function SetPlayerName sets the text of a UI element to the nickname of a
    /// Photon.Realtime.Player object.
    /// </summary>
    /// <param name="player">The player parameter is of type Photon.Realtime.Player, which represents a
    /// player in a Photon multiplayer game.</param>
    public void SetPlayerName(Photon.Realtime.Player player)
    {
        playerName.text = player.NickName;
    }

    /// <summary>
    /// The function ApplyLocalChanges changes the color of the playerName to red if the player is the
    /// local player.
    /// </summary>
    public void ApplyLocalChanges()
    {
        playerName.color = Color.red;
    }
}
