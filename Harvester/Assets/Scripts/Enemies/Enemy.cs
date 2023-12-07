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

    // Update is called once per frame
    void Update()
    {
        if (miscManager.currentAlivePlayers == 0)
            despawn();

        PlayAnimation();
        if (targetTransform == null && Time.time > wanderTime + _timeSinceLastWander)
        {
            _timeSinceLastWander = Time.time;
            Vector3 randomDirection = Random.insideUnitCircle * wanderDistance;
            randomDirection += transform.position;
            NavMeshHit navHit;
            NavMesh.SamplePosition(randomDirection, out navHit, wanderDistance, -1);
            agent.SetDestination(navHit.position);
        }
        else if (targetTransform != null)
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

    public void despawn() 
    {
        PhotonView view = PhotonView.Get(this);
        view.RPC("DestroyAll", RpcTarget.All);
    }

    [PunRPC]
    public void DestroyAll()
    {
        Destroy(gameObject);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && targetTransform == null)
        {
            print("ENTERED AREA");
            try
            {
                photonView.RPC("SetTarget", RpcTarget.All, (Vector2)collision.transform.position, collision.name);
            }
            catch { /*NOTHING*/ }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && targetTransform != null)
        {
            print("LEFT AREA");
            if (targetName == collision.name)
                photonView.RPC("SetTarget", RpcTarget.All, Vector2.zero, "");
        } 
    }

    [PunRPC]
    public void SetTarget(Vector2 givenTarget, string targetName)
    {
        this.target = givenTarget;
        this.targetName = targetName;
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
    public void SetMovingDirection(Vector2 movement)
    {
        if (movement.x < 0) // LEFT
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else // RIGHT
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }
}
