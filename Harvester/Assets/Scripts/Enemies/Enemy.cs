using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using WebSocketSharp;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    public Vector2 target;
    public string targetName;
    public Transform targetTransform;
    public NavMeshAgent agent;

    [Header("Attacking")]
    public float attackDistance;
    public float attackTime;
    public float attackChance;
    private float _timeOfLastAttack;

    [Header("Wandering")]
    public float wanderDistance;
    public float wanderTime;
    private float _timeSinceLastWander;

    [Header("Health")]
    public float curHealth;
    public Slider healthSlider;
    public float maxHealth;


    [Header("Animation")]
    public EnemyAnimManager anim;
    public bool moving;
    public bool attack;
    public bool hit;
    public bool die;
    public Vector2 previousVelocity;
    public float movingMagnitudeThreshold = 0.1f;

    [Header("Boss")]
    public bool boss;
    public int bossID;
    public BossManager bossManager;
    public MiscManager miscManager;

    [Header("Photon")]
    public PhotonView photonView;

/// <summary>
/// Initialization method called when the object is started.
/// </summary>
    private void Start()
    {
        photonView = PhotonView.Get(this);
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
        curHealth = maxHealth;
        bossManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<BossManager>();
        miscManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<MiscManager>();
    }

/// <summary>
/// Update method responsible for managing the dynamic behavior of the object.
/// </summary>
/// <remarks>
/// This method checks for the presence of active players and triggers the despawn mechanism if none are found.
/// It also handles the wandering behavior when there is no specified target, randomly setting a destination within a defined distance.
/// When a target is set, the object moves toward it and initiates attacks based on distance and a randomized chance.
/// </remarks>
    void Update()
    {
        if (miscManager.currentAlivePlayers == 0)
            despawn();

        PlayAnimation();
        if (targetTransform == null && Time.time > wanderTime + _timeSinceLastWander && gameObject.activeInHierarchy && gameObject != null)
        {
            _timeSinceLastWander = Time.time;
            Vector3 randomDirection = Random.insideUnitCircle * wanderDistance;
            randomDirection += transform.position;
            NavMeshHit navHit;
            NavMesh.SamplePosition(randomDirection, out navHit, wanderDistance, -1);
            try { agent.SetDestination(navHit.position); }
            catch { despawn();  }
        }
        else if (targetTransform != null && gameObject.activeInHierarchy && gameObject != null)
        {
            target = targetTransform.position;
            agent.SetDestination(target);
            if (Time.time < attackTime + _timeOfLastAttack)
                return;
            if (Vector2.Distance(target, transform.position) < attackDistance)
            {
                // add a random chance to make the attack a bit nicer
                if (Random.value > attackChance)
                    return;

                // attack the player
                _timeOfLastAttack = Time.time;
                attack = true;
                PhotonView playerPhotonView = PhotonView.Get(targetTransform.gameObject);
                playerPhotonView.RPC("DecreaseHealth", RpcTarget.All);
            }
        }
    }

/// <summary>
/// RPC method to apply damage to the object.
/// </summary>
/// <param name="damage">The amount of damage to be applied.</param>
    [PunRPC]
    public void TakeDamage(float damage)
    {
        curHealth = curHealth - damage > 0 ? curHealth - damage : 0;
        hit = true;
        healthSlider.value = curHealth;

        if (curHealth <= 0)
        {
            die = true;
            if (boss)
            { 
                if (PhotonNetwork.IsMasterClient)
                    bossManager.BossKilled(bossID);
            }
            Invoke("despawn", 0.2f);
        }
    }

/// <summary>
/// Initiates the despawning process for the object.
/// </summary>
    public void despawn() 
    {
        PhotonView view = PhotonView.Get(this);
        view.RPC("DestroyAll", RpcTarget.All);
    }

/// <summary>
/// RPC method to destroy the object for all networked players.
/// </summary>
    [PunRPC]
    public void DestroyAll()
    {
        Destroy(gameObject);
    }

/// <summary>
/// Triggered when another Collider2D enters this object's trigger zone.
/// </summary>
/// <param name="collision">The Collider2D entering the trigger zone.</param>
/// <remarks>
/// This method checks if the entering collider has the "Player" tag and if the object has no existing target.
/// If the conditions are met, it prints debug information, retrieves the player's position and name, and initiates the SetTarget RPC for all players.
/// </remarks>
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && targetTransform == null)
        {
            print("ENTERED AREA");
            print(collision.name);
            try
            {
                photonView.RPC("SetTarget", RpcTarget.All, (Vector2)collision.transform.position, collision.name);
            }
            catch { /*NOTHING*/ }
        }
    }

/// <summary>
/// Triggered when another Collider2D exits this object's trigger zone.
/// </summary>
/// <param name="collision">The Collider2D exiting the trigger zone.</param>
/// <remarks>
/// This method checks if the exiting collider has the "Player" tag and if the object has an existing target.
/// If the conditions are met, it prints debug information, compares the exiting player's name with the current target's name, 
/// and initiates the SetTarget RPC for all players if there is a match.
/// </remarks>
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && targetTransform != null)
        {
            print("LEFT AREA");
            if (targetName == collision.name)
                photonView.RPC("SetTarget", RpcTarget.All, Vector2.zero, "");
        } 
    }

/// <summary>
/// RPC method to set the target and associated properties for the object.
/// </summary>
/// <param name="givenTarget">The new target position.</param>
/// <param name="targetName">The name of the target.</param>
/// <remarks>
/// This method is invoked remotely to update the object's target position and name.
/// It also attempts to find the GameObject corresponding to the target name and sets the target transform accordingly.
/// If no valid target name is provided, the target transform is set to null.
/// </remarks>
    [PunRPC]
    public void SetTarget(Vector2 givenTarget, string targetName)
    {
        this.target = givenTarget;
        this.targetName = targetName;
        print(targetName);
        if (!string.IsNullOrEmpty(targetName))
        {
            var temp = GameObject.Find(targetName);
            if (temp)
                this.targetTransform = temp.transform;
            else
                targetTransform = null;
        }
        else
            targetTransform = null;
    }

/// <summary>
/// Plays the appropriate animation based on the object's current state.
/// </summary>
/// <remarks>
/// This method determines the object's state, such as moving, attacking, being hit, or dying, 
/// and plays the corresponding animation using the associated animation controller.
/// The movement direction is also updated, and the previous velocity is stored for non-moving states.
/// </remarks>
    public void PlayAnimation()
    {
        var velo = agent.velocity;
        moving = velo.magnitude > movingMagnitudeThreshold ? true : false;
        SetMovingDirection(velo);
        if (!moving)
            SetMovingDirection(previousVelocity);

        if (die)
        {
            anim.ChangeAnimationState(anim.Anims[4]);
        }
        else if (hit)
        {
            anim.ChangeAnimationState(anim.Anims[3]);
            if (anim.finished(anim.Anims[3]))
                hit = false;
        }
        else if (attack)
        {
            anim.ChangeAnimationState(anim.Anims[2]);
            if (anim.finished(anim.Anims[2]))
                attack = false;
        }
        else if (moving)
        {
            anim.ChangeAnimationState(anim.Anims[1]);
        }
        else
        {
            anim.ChangeAnimationState(anim.Anims[0]);
        }
        if (moving)
            previousVelocity = velo;
    }
/// <summary>
/// Sets the facing direction of the object based on the provided movement vector.
/// </summary>
/// <param name="movement">The movement vector indicating the object's direction.</param>
/// <remarks>
/// This method adjusts the object's scale to reflect the facing direction, 
/// flipping it horizontally when moving left and keeping it as is when moving right.
/// </remarks>
    public void SetMovingDirection(Vector2 movement)
    {
        if (movement.x < 0) // LEFT
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else // RIGHT
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }
}
