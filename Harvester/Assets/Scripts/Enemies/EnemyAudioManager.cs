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

    public void PlayAttack() { audioSource.PlayOneShot(attack); }
    public void PlayDie() { audioSource.PlayOneShot(die); }
}
