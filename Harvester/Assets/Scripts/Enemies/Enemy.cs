using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    public Transform target;
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
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
            return;

        PlayAnimation();
        if (target == null && Time.time > wanderTime + _timeSinceLastWander)
        {
            _timeSinceLastWander = Time.time;
            Vector3 randomDirection = Random.insideUnitCircle * wanderDistance;
            randomDirection += transform.position;
            NavMeshHit navHit;
            NavMesh.SamplePosition(randomDirection, out navHit, wanderDistance, -1);
            agent.SetDestination(navHit.position);
        }
        else if (target != null)
        {
            agent.SetDestination(target.position);
            if (Time.time < attackTime + _timeOfLastAttack)
                return;
            if (Vector2.Distance(target.position, transform.position) < attackDistance)
            {
                // add a random chance to make the attack a bit nicer
                if (Random.value > attackChance)
                    return;

                // attack the player
                _timeOfLastAttack = Time.time;
                attack = true;
                photonView.RPC("DecreaseHealth", RpcTarget.All);
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
        PhotonNetwork.Destroy(gameObject);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && target == null)
            photonView.RPC("SetTarget", RpcTarget.All, collision.transform);
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && target != null)
        { 
            if (target.GetInstanceID() == collision.transform.GetInstanceID())
                photonView.RPC("SetTarget", RpcTarget.All, collision.transform);
        } 
    }

    [PunRPC]
    public void SetTarget(Transform givenTarget)
    {
        target = givenTarget;
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
