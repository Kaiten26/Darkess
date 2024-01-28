using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    public float invincibilityDurationSeconds = 2.0f;
    public float knockbackSpeedX = 5.0f;
    public float knockbackSpeedY = 5.0f;
    private bool isInvincible = false;
    private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(int damage, Collider2D collider)
    {
        if (isInvincible) return;

        currentHealth -= damage;

        // Appliquez un effet de recul
        if (rb != null && collider != null)
        {
            Vector2 knockbackDirection = (transform.position - collider.transform.position).normalized;
            Vector2 knockback = new Vector2(knockbackDirection.x * knockbackSpeedX, knockbackSpeedY);
            rb.AddForce(knockback, ForceMode2D.Impulse);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Commencez la période d'invincibilité
            StartCoroutine(BecomeTemporarilyInvincible());
        }
    }
    public bool IsInvincible
    {
        get { return isInvincible; }
    }

    IEnumerator BecomeTemporarilyInvincible()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDurationSeconds);
        isInvincible = false;
    }

    void Die()
    {
        // Ajoutez ici la logique de mort
    }
}

