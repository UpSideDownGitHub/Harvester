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

    public void PlayHitAxe() { audioSource.PlayOneShot(hitAxe); }
    public void PlayHitPick() { audioSource.PlayOneShot(hitPick); }
    public void PlayHitSword() { audioSource.PlayOneShot(hitSword); }
    public void PlayDie() { audioSource.PlayOneShot(die); }
    public void PlayEat() { audioSource.PlayOneShot(eat); }
    public void PlayLoseHeart() { audioSource.PlayOneShot(loseHeart); }
    public void PlayPickup() { audioSource.PlayOneShot(pickup, 0.5f); }
}
