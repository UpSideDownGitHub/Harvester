using FishNet.Object;
using FishNet.Object.Synchronizing;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.UI;

public class Enemy : NetworkBehaviour
{
    [SyncVar]public Transform target;
    public NavMeshAgent agent;

    [Header("Attacking")]
    public float attackDistance;
    public float attackTime;
    private float _timeOfLastAttack;

    [Header("Wandering")]
    public float wanderDistance;
    public float wanderTime;
    private float _timeSinceLastWander;

    [Header("Health")]
    [SyncVar(OnChange = "syncHealth")]public float curHealth;
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

    private void Start()
    {
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
        curHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (!base.IsOwner)
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
                // attack the player
                _timeOfLastAttack = Time.time;
                attack = true;
                if (target.GetComponent<PlayerHolding>())
                    target.GetComponent<PlayerHolding>().SetHealthChange(!target.GetComponent<PlayerHolding>().health);
            }
        }
    }

    public void syncHealth(float oldValue, float newValue, bool asServer)
    {
        if (asServer)
            return;

        healthSlider.value = newValue;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamage(float damage)
    {
        curHealth = curHealth - damage > 0 ? curHealth - damage : 0;
        hit = true;
        healthSlider.value = curHealth;

        if (curHealth <= 0)
        {
            die = true;
            Invoke("despawn", 0.2f);
        }
    }

    public void despawn() { DespawnObject(gameObject); }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnObject(GameObject toDestroy)
    {
        ServerManager.Despawn(toDestroy);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && target == null)
            SetTarget(collision.transform);
    }
    
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && target.GetInstanceID() == collision.transform.GetInstanceID())
            SetTarget(null);
    }

    [ServerRpc(RequireOwnership = false)]
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
