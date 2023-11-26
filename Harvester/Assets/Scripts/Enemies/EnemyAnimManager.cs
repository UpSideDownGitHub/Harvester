using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyAnimManager : MonoBehaviour
{
    [Header("Animations")]
    public Animator anim;
    public string currentState;
    PhotonView view;
    // Idle, Walk, Attack, Hit, Die
    public string[] Anims;


    public void Start()
    {
        view = PhotonView.Get(this);
    }

    public void ChangeAnimationState(string newState)
    {
        if (currentState == newState)
            return;
        view.RPC("PlayAnimation", RpcTarget.All, newState);
    }

    [PunRPC]
    public void PlayAnimation(string state)
    {
        anim.Play(state);
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
