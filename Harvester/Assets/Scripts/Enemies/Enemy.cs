using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.Image;

public class Enemy : MonoBehaviour
{
    public Transform target;
    public NavMeshAgent agent;

    [Header("Attacking")]
    public float attackDistance;
    public float attackTime;
    private float _timeOfLastAttack;

    [Header("Wandering")]
    public float wanderDistance;
    public float wanderTime;
    private float _timeSinceLastWander;

    // Update is called once per frame
    void Update()
    {
        if (target == null && Time.time > wanderTime + _timeSinceLastWander)
        {
            _timeSinceLastWander = Time.time;
            Vector3 randomDirection = Random.insideUnitCircle * wanderDistance;
            randomDirection += transform.position;
            NavMeshHit navHit;
            NavMesh.SamplePosition(randomDirection, out navHit, wanderDistance, -1);
            agent.SetDestination(navHit.position);
        }
        else
        {
            agent.SetDestination(target.position);
            if (Time.time < attackTime + _timeOfLastAttack)
                return;
            if (Vector2.Distance(target.position, transform.position) < attackDistance)
            {
                // attack the player
                target.GetComponent<Player>().DecreaseHealth();
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && target == null)
            target = collision.transform;
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && target.GetInstanceID() == collision.GetInstanceID())
            target = null;
    }
}
