using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class PlayerAnimManager : MonoBehaviour
{
    [Header("Animations")]
    public Animator anim;

    public string currentState;

    public const string CarryWalk_Right = "CarryWalk_Right";
    public const string CarryWalk_Left = "CarryWalk_Left";
    public const string CarryWalk_Up = "CarryWalk_Up";
    public const string CarryWalk_Down = "CarryWalk_Down";

    public const string CarryIdle_Right = "CarryIdle_Right";
    public const string CarryIdle_Left = "CarryIdle_Left";
    public const string CarryIdle_Up = "CarryIdle_Up";
    public const string CarryIdle_Down = "CarryIdle_Down";

    public const string Idle_Right = "Idle_Right";
    public const string Idle_Left = "Idle_Left";
    public const string Idle_Up = "Idle_Up";
    public const string Idle = "Idle";

    public const string Walk_Right = "Walk_Right";
    public const string Walk_Left = "Walk_Left";
    public const string Walk_Up = "Walk_Up";
    public const string Walk_Down = "Walk_Down";

    public const string Attack_Right = "Attack_Right";
    public const string Attack_Left = "Attack_Left";
    public const string Attack_Up = "Attack_Up";
    public const string Attack_Down = "Attack_Down";

    public const string Mine_Right = "Mine_Right";
    public const string Mine_Left = "Mine_Left";
    public const string Mine_Up = "Mine_Up";
    public const string Mine_Down = "Mine_Down";

    public const string Axe_Right = "Axe_Right";
    public const string Axe_Left = "Axe_Left";
    public const string Axe_Up = "Axe_Up";
    public const string Axe_Down = "Axe_Down";

    public const string Hit_Right = "Hit_Right";
    public const string Hit_Left = "Hit_Left";
    public const string Hit_Up = "Hit_Up";
    public const string Hit_Down = "Hit_Down";

    public const string Die = "Die";

    public const string Pickup = "Pickup";

/// <summary>
/// Changes the player's animation state and synchronizes it across the network.
/// </summary>
/// <param name="newState">The new animation state to set.</param>
/// <remarks>
/// This method compares the current animation state with the new state and uses Photon RPCs to synchronize the change
/// across the network for all players.
/// </remarks>
    public void ChangeAnimationState(string newState)
    {
        if (currentState == newState)
            return;
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("PlayAnimation", RpcTarget.All, newState);
    }

/// <summary>
/// Changes the player's animation state if the current state is not in the list of similar states.
/// </summary>
/// <param name="newState">The new animation state to set.</param>
/// <param name="similar">A list of similar states that prevent the change if the current state matches any of them.</param>
/// <returns>True if the animation state is changed; false otherwise.</returns>
/// <remarks>
/// This method checks if the current animation state is in the list of similar states. If not, it changes the state
/// and synchronizes the change across the network using Photon RPCs.
/// </remarks>
    public bool ChangeAnimationState(string newState, List<string> similar)
    {
        for (int i = 0; i < similar.Count; i++)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(similar[i]))
                return false;
        }
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("PlayAnimation", RpcTarget.All, newState);
        return true;
    }

/// <summary>
/// Plays the specified animation state for the player and synchronizes it across the network.
/// </summary>
/// <param name="state">The animation state to play.</param>
/// <remarks>
/// This method uses Photon RPCs to synchronize the animation state change across the network for all players.
/// </remarks>
    [PunRPC]
    public void PlayAnimation(string state)
    {
        currentState = state;
        anim.Play(state);
    }

/// <summary>
/// Checks if the specified animation state has finished playing.
/// </summary>
/// <param name="name">The name of the animation state to check.</param>
/// <returns>True if the animation state has finished playing; false otherwise.</returns>
/// <remarks>
/// This method checks if the specified animation state has finished playing based on the normalized time.
/// </remarks>
    public bool finished(string name)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(name) &&
            anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            return true;
        return false;
    }
}
