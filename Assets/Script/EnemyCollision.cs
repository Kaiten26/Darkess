using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCollision : MonoBehaviour
{
    public int damageOnCollision = 10;
    private bool isInvincible;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null && !playerHealth.IsInvincible)
            {
                playerHealth.TakeDamage(damageOnCollision, collision.collider);
            }
        }
    }
}

