using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class PlayerAudiomanager : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hitAxe;
    public AudioClip hitPick;
    public AudioClip hitSword;
    public AudioClip die;
    public AudioClip eat;
    public AudioClip loseHeart;
    public AudioClip pickup;
    public AudioClip openMenu;
    public AudioClip closeMenu;

/// <summary>
/// Plays the hit sound effect for the axe.
/// </summary>
    public void PlayHitAxe() { audioSource.PlayOneShot(hitAxe); }
/// <summary>
/// Plays the hit sound effect for the pickaxe.
/// </summary>
    public void PlayHitPick() { audioSource.PlayOneShot(hitPick); }
/// <summary>
/// Plays the hit sound effect for the sword.
/// </summary>
    public void PlayHitSword() { audioSource.PlayOneShot(hitSword); }
/// <summary>
/// Plays the die sound effect.
/// </summary>
    public void PlayDie() { audioSource.PlayOneShot(die); }
/// <summary>
/// Plays the eat sound effect.
/// </summary>
    public void PlayEat() { audioSource.PlayOneShot(eat); }
/// <summary>
/// Plays the lose heart sound effect.
/// </summary>
    public void PlayLoseHeart() { audioSource.PlayOneShot(loseHeart); }
/// <summary>
/// Plays the pickup sound effect with reduced volume.
/// </summary>
    public void PlayPickup() { audioSource.PlayOneShot(pickup, 0.3f); }
/// <summary>
/// Plays the open menu sound effect.
/// </summary>
    public void PlayOpenMenu() { audioSource.PlayOneShot(openMenu); }
/// <summary>
/// Plays the close menu sound effect.
/// </summary>
    public void PlayCloseMenu() { audioSource.PlayOneShot(closeMenu); }
}
