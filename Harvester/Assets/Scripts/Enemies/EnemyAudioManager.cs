using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyAudioManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip attack;
    public AudioClip die;

/// <summary>
/// Plays the attack sound effect using the object's audio source.
/// </summary>
    public void PlayAttack() { audioSource.PlayOneShot(attack); }
/// <summary>
/// Plays the die sound effect using the object's audio source.
/// </summary>
    public void PlayDie() { audioSource.PlayOneShot(die); }
}
