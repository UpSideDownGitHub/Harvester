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


/// <summary>
/// Initialization method called when the object is started.
/// </summary>
/// <remarks>
/// This method retrieves the PhotonView component for network synchronization.
/// </remarks>
    public void Start()
    {
        view = PhotonView.Get(this);
    }

/// <summary>
/// Changes the animation state of the object and synchronizes the change across the network.
/// </summary>
/// <param name="newState">The new animation state to set.</param>
/// <remarks>
/// This method compares the current animation state with the new state and triggers a network RPC to play the specified animation for all players if they differ.
/// </remarks>
    public void ChangeAnimationState(string newState)
    {
        if (currentState == newState)
            return;
        view.RPC("PlayAnimation", RpcTarget.All, newState);
    }

/// <summary>
/// RPC method to play the specified animation state.
/// </summary>
/// <param name="state">The animation state to play.</param>
/// <remarks>
/// This method is invoked remotely to play the specified animation state using the associated animation controller.
/// It also updates the current animation state for local tracking.
/// </remarks>
    [PunRPC]
    public void PlayAnimation(string state)
    {
        anim.Play(state);
        currentState = state;
    }

/// <summary>
/// Checks if the specified animation state has finished playing.
/// </summary>
/// <param name="name">The name of the animation state to check.</param>
/// <returns>
/// True if the animation state matches the provided name and has reached or surpassed its normalized end time, indicating completion.
/// False otherwise.
/// </returns>
    public bool finished(string name)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(name) &&
            anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            return true;
        return false;
    }
}
