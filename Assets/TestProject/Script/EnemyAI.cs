using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform[] patrolPoints; // Points de patrouille
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;
    public float chaseDistance = 5f; // Distance de détection du joueur

    private Transform player;
    private int currentPointIndex = 0;
    private bool chasingPlayer = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Trouve le joueur dans la scène
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < chaseDistance)
        {
            chasingPlayer = true;
        }
        else
        {
            chasingPlayer = false;
        }

        if (chasingPlayer)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        Transform patrolPoint = patrolPoints[currentPointIndex];

        if (Vector2.Distance(transform.position, patrolPoint.position) < 0.2f)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        }
        else
        {
            MoveTowards(patrolPoint.position, patrolSpeed);
        }
    }

    void ChasePlayer()
    {
        MoveTowards(player.position, chaseSpeed);
    }

    void MoveTowards(Vector2 target, float speed)
    {
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }
}

