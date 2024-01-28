using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public int damage = 10; // Le montant des dégâts infligés

    void OnTriggerEnter2D(Collider2D collision)
    {
        Health enemyHealth = collision.GetComponent<Health>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage, collision);
        }
    }
}
