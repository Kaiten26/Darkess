using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float attackInterval = 2f; // Intervalle entre les cycles d'attaque
    public float attackCooldown = 5f; // Cooldown entre deux attaques
    public float vanishTime = 1f;
    public Transform[] attackPoints;
    public Transform airAttackPoint;
    private int currentAttackPointIndex = 0;

    public Transform slamAttackPoint; // Point de position pour le slam attack
    public Transform bossDeathPoint; // Point de t�l�portation pour la mort du boss

    // Listes publiques de hitboxes pour chaque type d'attaque
    public List<Transform> sliceAttackHitboxes = new List<Transform>();
    public List<Transform> slamAttackHitboxes = new List<Transform>();
    public List<Transform> vanishAttackHitboxes = new List<Transform>();

    public float attackRange = 1f;
    public float attackDamage = 5f;

    public float maxHP = 100f;
    private float currentHP;

    private Animator animator;
    private Rigidbody2D rb;
    private bool isAttacking = false;
    private bool isVanishing = false;
    private bool canMove = true; // Indique si le boss peut bouger
    private Transform player;
    private float lastAttackTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHP = maxHP;
        lastAttackTime = -attackCooldown; // Initialisation pour permettre une attaque imm�diate
        StartCoroutine(AttackCycle());
    }

    void Update()
    {
        if (!isAttacking && !isVanishing && canMove)
        {
            MoveTowardsPlayer();
            animator.SetBool("Move", true);
        }
        else
        {
            animator.SetBool("Move", false);
        }
    }

    void MoveTowardsPlayer()
    {
        if (player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = direction * moveSpeed;

            // Flip le boss en direction du joueur
            if (direction.x > 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (direction.x < 0)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }

    IEnumerator AttackCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(attackInterval);

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                int attackType = Random.Range(0, 3);
                switch (attackType)
                {
                    case 0:
                        StartCoroutine(SliceAttack());
                        break;
                    case 1:
                        StartCoroutine(SlamAttack());
                        break;
                    case 2:
                        StartCoroutine(VanishAttack());
                        break;
                }
                lastAttackTime = Time.time; // Mise � jour du temps de la derni�re attaque
            }
        }
    }

    IEnumerator SliceAttack()
    {
        isAttacking = true;
        canMove = true; // Permet au boss de bouger pendant cette attaque
        animator.SetTrigger("Attack1");

        // Attaque � la frame 3 (0.3 secondes � 10 FPS)
        yield return new WaitForSeconds(0.3f);
        PerformAttack(sliceAttackHitboxes);

        // Attaque � la frame 10 (1.0 seconde � 10 FPS)
        yield return new WaitForSeconds(0.7f);
        PerformAttack(sliceAttackHitboxes);

        // Attendre que l'animation de 16 frames (environ 1.6 secondes) se termine
        yield return new WaitForSeconds(0.6f);

        isAttacking = false;
        canMove = true; // R�autorise le mouvement apr�s l'attaque
    }

    IEnumerator SlamAttack()
    {
        isAttacking = true;
        canMove = false; // Emp�che le boss de bouger pendant cette attaque
        rb.velocity = Vector2.zero;

        // T�l�portation au point d'attaque de slam
        if (slamAttackPoint != null)
        {
            transform.position = slamAttackPoint.position;
            Debug.Log("Boss teleported to slam attack point: " + transform.position);
        }

        // D�clencher l'animation SlamAttack
        animator.SetTrigger("SlamAttack");

        // Attaque � la frame 28 (2.8 secondes � 10 FPS)
        yield return new WaitForSeconds(2.8f);
        PerformAttack(slamAttackHitboxes);

        // Attaque � la frame 31 (3.1 secondes � 10 FPS)
        yield return new WaitForSeconds(0.3f);
        PerformAttack(slamAttackHitboxes);

        // Attendre que l'animation de 48 frames (environ 4.8 secondes) se termine
        yield return new WaitForSeconds(1.7f);

        isAttacking = false;
        canMove = true; // R�autorise le mouvement apr�s l'attaque
    }

    IEnumerator VanishAttack()
    {
        if (attackPoints.Length == 0)
        {
            Debug.LogError("No attack points set for VanishAttack!");
            yield break;
        }

        isVanishing = true;
        canMove = false; // Emp�che le boss de bouger pendant cette attaque
        rb.velocity = Vector2.zero; // Stop moving
        animator.SetTrigger("Vanish");
        yield return new WaitForSeconds(0.5f);

        // Ensure the index is within the bounds of the array
        currentAttackPointIndex = currentAttackPointIndex % attackPoints.Length;
        transform.position = attackPoints[currentAttackPointIndex].position;
        Debug.Log("Boss teleported to attack point: " + transform.position);
        currentAttackPointIndex = (currentAttackPointIndex + 1) % attackPoints.Length;

        yield return new WaitForSeconds(vanishTime);

        // R�appara�t
        animator.SetTrigger("Appear");
        yield return new WaitForSeconds(0.5f);

        // Lancer l'animation de VanishAttack
        animator.SetTrigger("VanishAttack");

        // Attaque � la frame 2 (0.2 secondes � 10 FPS)
        yield return new WaitForSeconds(0.2f);
        PerformAttack(vanishAttackHitboxes);

        // Attaque � la frame 4 (0.4 secondes � 10 FPS)
        yield return new WaitForSeconds(0.2f);
        PerformAttack(vanishAttackHitboxes);

        // Attendre que l'animation de 9 frames (environ 0.9 seconde) se termine
        yield return new WaitForSeconds(0.5f);

        isVanishing = false;
        canMove = true; // R�autorise le mouvement apr�s l'attaque
    }

    void PerformAttack(List<Transform> hitboxes)
    {
        foreach (Transform hitbox in hitboxes)
        {
            Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(hitbox.position, attackRange);
            foreach (Collider2D hitPlayer in hitPlayers)
            {
                if (hitPlayer.tag == "Player")
                {
                    hitPlayer.GetComponent<CharacterController2D>().ApplyDamage(attackDamage, transform.position);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        foreach (Transform hitbox in sliceAttackHitboxes)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hitbox.position, attackRange);
        }

        foreach (Transform hitbox in slamAttackHitboxes)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(hitbox.position, attackRange);
        }

        foreach (Transform hitbox in vanishAttackHitboxes)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(hitbox.position, attackRange);
        }
    }

    public void ApplyDamage(float damage)
    {
        currentHP -= damage; // R�duire les HP du boss
        Debug.Log("Boss takes " + damage + " damage. Current HP: " + currentHP);
        animator.SetTrigger("Hit");

        if (currentHP <= 0)
        {
            StartCoroutine(Die()); // Appeler la m�thode Die avec une coroutine
        }
    }

    IEnumerator Die()
    {
        isVanishing = true;
        canMove = false; // Emp�che le boss de bouger pendant cette attaque
        rb.velocity = Vector2.zero; // Stop moving
        animator.SetTrigger("Vanish");
        yield return new WaitForSeconds(0.5f);

        // Teleport to bossDeathPoint
        if (bossDeathPoint != null)
        {
            transform.position = bossDeathPoint.position;
            Debug.Log("Boss teleported to death point: " + transform.position);
            animator.SetTrigger("Appear");
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            Debug.LogError("bossDeathPoint is not assigned!");
        }

        // Disable collisions
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }
        Debug.Log("Boss collisions disabled");

        // Play death animation
        animator.SetTrigger("Die");
        Debug.Log("Boss playing death animation");

        // D�sactivez le script BossController pour emp�cher tout mouvement ou action suppl�mentaire
        this.enabled = false;

        isVanishing = false;
        // Le boss ne bouge plus apr�s �tre mort
    }
}
