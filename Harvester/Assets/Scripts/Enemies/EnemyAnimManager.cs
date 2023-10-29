using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Example.Scened;
using FishNet.Object.Synchronizing;

public class EnemyAnimManager : NetworkBehaviour
{
    [Header("Animations")]
    public Animator anim;

    [SyncVar(OnChange = "PlayAnim")] public string currentState;

    // Idle, Walk, Attack, Hit, Die
    public string[] Anims;

    public void PlayAnim(string oldValue, string newValue, bool asServer)
    {
        if (asServer)
            return;

        anim.Play(newValue);
    }

    public void ChangeAnimationState(string newState)
    {
        if (currentState == newState)
            return;
        anim.Play(newState);
        SetCurrentState(newState);
    }

    [ServerRpc(RequireOwnership = false)]
    void SetCurrentState(string state)
    {
        currentState = state;
    }

    public bool finished(string name)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(name) &&
            anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            return true;
        return false;
    }
}
