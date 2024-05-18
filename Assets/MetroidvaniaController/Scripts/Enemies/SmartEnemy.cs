using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartEnemy : MonoBehaviour
{
    // Event de destruction
    public delegate void OnDestroyDelegate();
    public event OnDestroyDelegate OnDestroyEvent;

    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true;  // For determining which way the player is currently facing.

    public float life = 10;

    private bool facingRight = true;

    public float speed = 5f;
    public float patrolSpeed = 2f;  // Vitesse de patrouille

    public bool isInvincible = false;
    private bool isHitted = false;

    [SerializeField] private float m_DashForce = 25f;
    private bool isDashing = false;

    private float distToTarget;
    private float distToTargetY;
    public float meleeDist = 1.5f;
    public float rangeDist = 5f;
    private bool canAttack = true;
    private Transform attackCheck;
    public float dmgValue = 4;

    public GameObject target;  // Cible actuelle de l'ennemi

    public float attackSpeed = 1f; // Vitesse d'attaque

    // Liste de hitboxes pour l'attaque
    public List<Transform> attackHitboxes = new List<Transform>();
    public float attackRange = 1f;
    public float attackDamage = 4f;

    // private float randomDecision = 0;
    private bool doOnceDecision = true;
    private bool endDecision = false;
    private Animator anim;

    public GameObject soulPrefab;

    // Nouvelle variable pour la distance de proximité
    public float proximityDistance = 10f;

    // Variables pour la patrouille
    public float patrolRange = 5f;
    private Vector3 initialPosition;
    private bool patrollingRight = true;

    // Variables pour les couleurs des gizmos
    public Color patrolRangeColor = Color.blue;
    public Color proximityRangeColor = Color.red;

    void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        attackCheck = transform.Find("AttackCheck").transform;
        anim = GetComponent<Animator>();
        initialPosition = transform.position;
    }

    void FixedUpdate()
    {
        // Recherche de la cible la plus proche
        target = FindClosestTarget();

        if (life <= 0)
        {
            StartCoroutine(DestroyEnemy());
        }
        else if (target != null)
        {
            if (isDashing)
            {
                m_Rigidbody2D.velocity = new Vector2(transform.localScale.x * m_DashForce, 0);
            }
            else if (!isHitted)
            {
                distToTarget = target.transform.position.x - transform.position.x;
                distToTargetY = target.transform.position.y - transform.position.y;

                if (Mathf.Abs(distToTarget) < 0.25f)
                {
                    GetComponent<Rigidbody2D>().velocity = new Vector2(0f, m_Rigidbody2D.velocity.y);
                    anim.SetBool("IsWaiting", true);
                }
                else if (Mathf.Abs(distToTarget) > 0.25f && Mathf.Abs(distToTarget) < meleeDist && Mathf.Abs(distToTargetY) < 2f)
                {
                    GetComponent<Rigidbody2D>().velocity = new Vector2(0f, m_Rigidbody2D.velocity.y);
                    if ((distToTarget > 0f && transform.localScale.x < 0f) || (distToTarget < 0f && transform.localScale.x > 0f))
                        Flip();
                    if (canAttack)
                    {
                        StartCoroutine(Attack());
                    }
                }
                else if (Mathf.Abs(distToTarget) > meleeDist && Mathf.Abs(distToTarget) < rangeDist)
                {
                    anim.SetBool("IsWaiting", false);
                    m_Rigidbody2D.velocity = new Vector2(distToTarget / Mathf.Abs(distToTarget) * speed, m_Rigidbody2D.velocity.y);
                }
                else
                {
                    if (!endDecision)
                    {
                        if ((distToTarget > 0f && transform.localScale.x < 0f) || (distToTarget < 0f && transform.localScale.x > 0f))
                            Flip();

                        Run();
                        Jump();
                        StartCoroutine(Dash());
                        Idle();
                    }
                    else
                    {
                        endDecision = false;
                    }
                }
            }
            else if (isHitted)
            {
                if ((distToTarget > 0f && transform.localScale.x > 0f) || (distToTarget < 0f && transform.localScale.x < 0f))
                {
                    Flip();
                    StartCoroutine(Dash());
                }
                else
                    StartCoroutine(Dash());
            }
        }

        if (transform.localScale.x * m_Rigidbody2D.velocity.x > 0 && !m_FacingRight && life > 0)
        {
            // ... flip the player.
            Flip();
        }
        // Otherwise if the input is moving the player left and the player is facing right...
        else if (transform.localScale.x * m_Rigidbody2D.velocity.x < 0 && m_FacingRight && life > 0)
        {
            // ... flip the player.
            Flip();
        }

        // Si aucune cible n'est proche, patrouiller
        if (target == null)
        {
            Patrol();
        }
    }

    GameObject FindClosestTarget()
    {
        GameObject closestTarget = null;
        float closestDistance = Mathf.Infinity;

        // Recherche du joueur
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer < closestDistance)
            {
                closestDistance = distanceToPlayer;
                closestTarget = player;
            }
        }

        // Recherche des alliés
        GameObject[] allies = GameObject.FindGameObjectsWithTag("Allies");
        foreach (GameObject ally in allies)
        {
            float distanceToAlly = Vector3.Distance(transform.position, ally.transform.position);
            if (distanceToAlly < closestDistance)
            {
                closestDistance = distanceToAlly;
                closestTarget = ally;
            }
        }

        // Vérifie si la cible la plus proche est dans la distance de proximité
        if (closestDistance <= proximityDistance)
        {
            return closestTarget;
        }

        return null;
    }

    void Patrol()
    {
        if (patrollingRight)
        {
            m_Rigidbody2D.velocity = new Vector2(patrolSpeed, m_Rigidbody2D.velocity.y);
            if (transform.position.x >= initialPosition.x + patrolRange)
            {
                patrollingRight = false;
                Flip();
            }
        }
        else
        {
            m_Rigidbody2D.velocity = new Vector2(-patrolSpeed, m_Rigidbody2D.velocity.y);
            if (transform.position.x <= initialPosition.x - patrolRange)
            {
                patrollingRight = true;
                Flip();
            }
        }
    }

    void Flip()
    {
        // Switch the way the player is labelled as facing.
        facingRight = !facingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public void ApplyDamage(float damage)
    {
        if (!isInvincible)
        {
            float direction = damage / Mathf.Abs(damage);
            damage = Mathf.Abs(damage);
            anim.SetTrigger("Hit");  // Update this line
            life -= damage;
            transform.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
            transform.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(direction * 300f, 100f));
            StartCoroutine(HitTime());
        }
    }

    IEnumerator Attack()
    {
        canAttack = false;
        anim.SetTrigger("Attack");

        // Attendre jusqu'à la frame 6 (0.6 secondes à 10 FPS)
        yield return new WaitForSeconds(0.6f);

        PerformAttack();

        // Attendre que l'attaque soit terminée (1 seconde en total à 10 FPS)
        yield return new WaitForSeconds(0.4f);

        canAttack = true;
    }

    void PerformAttack()
    {
        foreach (Transform hitbox in attackHitboxes)
        {
            Collider2D[] hitTargets = Physics2D.OverlapCircleAll(hitbox.position, attackRange);
            foreach (Collider2D target in hitTargets)
            {
                if (target.tag == "Player" || target.tag == "Allies")
                {
                    target.GetComponent<CharacterController2D>().ApplyDamage(attackDamage, transform.position);
                }
            }
        }
    }

    public void Run()
    {
        anim.SetBool("IsWaiting", false);
        m_Rigidbody2D.velocity = new Vector2(distToTarget / Mathf.Abs(distToTarget) * speed, m_Rigidbody2D.velocity.y);
        if (doOnceDecision)
            StartCoroutine(NextDecision(0.5f));
    }
    public void Jump()
    {
        Vector3 targetVelocity = new Vector2(distToTarget / Mathf.Abs(distToTarget) * speed, m_Rigidbody2D.velocity.y);
        Vector3 velocity = Vector3.zero;
        m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref velocity, 0.05f);
        if (doOnceDecision)
        {
            anim.SetBool("IsWaiting", false);
            m_Rigidbody2D.AddForce(new Vector2(0f, 850f));
            StartCoroutine(NextDecision(1f));
        }
    }

    public void Idle()
    {
        m_Rigidbody2D.velocity = new Vector2(0f, m_Rigidbody2D.velocity.y);
        if (doOnceDecision)
        {
            anim.SetBool("IsWaiting", true);
            StartCoroutine(NextDecision(1f));
        }
    }

    public void EndDecision()
    {
        // randomDecision = Random.Range(0.0f, 1.0f);
        endDecision = true;
    }

    IEnumerator HitTime()
    {
        isInvincible = true;
        isHitted = true;
        yield return new WaitForSeconds(0.1f);
        isHitted = false;
        isInvincible = false;
    }

    IEnumerator Dash()
    {
        anim.SetTrigger("StartDash");  // Update this line
        isDashing = true;
        yield return new WaitForSeconds(0.1f);
        isDashing = false;
        anim.ResetTrigger("StartDash");  // Update this line
        EndDecision();
    }

    IEnumerator NextDecision(float time)
    {
        doOnceDecision = false;
        yield return new WaitForSeconds(time);
        EndDecision();
        doOnceDecision = true;
        anim.SetBool("IsWaiting", false);
    }

    IEnumerator DestroyEnemy()
    {
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(1f, 0.25f);
        capsule.offset = new Vector2(0f, -0.8f);
        capsule.direction = CapsuleDirection2D.Horizontal;
        transform.GetComponent<Animator>().SetBool("IsDead", true);
        yield return new WaitForSeconds(0.25f);
        m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
        yield return new WaitForSeconds(1f);

        // Instancier l'entité "Soul" à la position de cet ennemi
        Instantiate(soulPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);

        // Invoke OnDestroyEvent when the enemy is destroyed
        OnDestroyEvent?.Invoke();
    }

    // Méthode pour dessiner les gizmos dans l'éditeur
    private void OnDrawGizmos()
    {
        // Dessine la portée de patrouille en bleu
        Gizmos.color = patrolRangeColor;
        Gizmos.DrawWireSphere(initialPosition, patrolRange);

        // Dessine la portée de détection en rouge
        Gizmos.color = proximityRangeColor;
        Gizmos.DrawWireSphere(transform.position, proximityDistance);

        // Dessine les hitboxes d'attaque
        Gizmos.color = Color.green;
        foreach (Transform hitbox in attackHitboxes)
        {
            Gizmos.DrawWireSphere(hitbox.position, attackRange);
        }
    }
}
