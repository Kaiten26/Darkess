using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Allies : MonoBehaviour
{
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true;  // For determining which way the ally is currently facing.

    public float life = 10;

    private bool facingRight = true;

    public float speed = 5f;

    public bool isInvincible = false;
    private bool isHitted = false;

    [SerializeField] private float m_DashForce = 25f;
    private bool isDashing = false;

    public GameObject enemy;
    public GameObject player;
    private float distToTarget;
    private float distToTargetY;
    public float meleeDist = 1.5f;
    public float rangeDist = 5f;
    public float followDist = 2f;
    private bool canAttack = true;
    private Transform attackCheck;
    public float dmgValue = 4;

    public float attackSpeed = 1f; // Vitesse d'attaque

    public GameObject throwableObject;

    private float randomDecision = 0;
    private bool doOnceDecision = true;
    private bool endDecision = false;
    private Animator anim;

    // Liste de hitboxes pour les attaques
    public List<Transform> attackHitboxes = new List<Transform>();
    public float attackRange = 1f; // Portée de l'attaque

    void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        attackCheck = transform.Find("AttackCheck").transform;
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player"); // Assurez-vous que le joueur a le tag "Player"
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (life <= 0)
        {
            StartCoroutine(DestroyAlly());
        }

        FindClosestEnemy();

        if (enemy != null)
        {
            if (isDashing)
            {
                m_Rigidbody2D.velocity = new Vector2(transform.localScale.x * m_DashForce, 0);
            }
            else if (!isHitted)
            {
                distToTarget = enemy.transform.position.x - transform.position.x;
                distToTargetY = enemy.transform.position.y - transform.position.y;

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
                        StartCoroutine(PerformAttack());
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
                }
            }
        }

        if (enemy != null)
        {
            FollowTarget(enemy);
        }
        else if (player != null && enemy == null)
        {
            FollowTarget(player);
        }
        else
        {
            Idle();
        }

        if (transform.localScale.x * m_Rigidbody2D.velocity.x > 0 && !m_FacingRight && life > 0)
        {
            // ... flip the ally.
            Flip();
        }
        else if (transform.localScale.x * m_Rigidbody2D.velocity.x < 0 && m_FacingRight && life > 0)
        {
            // ... flip the ally.
            Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
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

    IEnumerator PerformAttack()
    {
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(0.6f); // Attendre la frame 6 à 10 FPS
        ActivateHitboxes();
        yield return new WaitForSeconds(0.4f); // Attendre la fin de l'attaque
        DeactivateHitboxes();
        StartCoroutine(WaitToAttack(1f / attackSpeed));
    }

    void ActivateHitboxes()
    {
        foreach (Transform hitbox in attackHitboxes)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(hitbox.position, attackRange);
            foreach (Collider2D hitEnemy in hitEnemies)
            {
                if (hitEnemy.tag == "Enemy")
                {
                    hitEnemy.GetComponent<SmartEnemy>().ApplyDamage(dmgValue);
                }
            }
        }
    }

    void DeactivateHitboxes()
    {
        // Cette fonction peut être utilisée pour désactiver les hitboxes si nécessaire
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
        anim.SetBool("IsWaiting", true);
    }

    public void FollowTarget(GameObject target)
    {
        if (life <= 0)
        {
            return;
        }

        distToTarget = target.transform.position.x - transform.position.x;
        distToTargetY = target.transform.position.y - transform.position.y;

        if (Mathf.Abs(distToTarget) > followDist)
        {
            anim.SetBool("IsWaiting", false);
            m_Rigidbody2D.velocity = new Vector2(distToTarget / Mathf.Abs(distToTarget) * speed, m_Rigidbody2D.velocity.y);
            if ((distToTarget > 0f && transform.localScale.x < 0f) || (distToTarget < 0f && transform.localScale.x > 0f))
                Flip();
        }
        else
        {
            Idle();
        }
    }

    public void EndDecision()
    {
        randomDecision = Random.Range(0.0f, 1.0f);
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

    IEnumerator WaitToAttack(float time)
    {
        canAttack = false;
        yield return new WaitForSeconds(time);
        canAttack = true;
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

    IEnumerator DestroyAlly()
    {
        m_Rigidbody2D.constraints = RigidbodyConstraints2D.FreezePosition;

        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(1f, 0.25f);
        capsule.offset = new Vector2(0f, -0.8f);
        capsule.direction = CapsuleDirection2D.Horizontal;
        transform.GetComponent<Animator>().SetBool("IsDead", true);
        yield return new WaitForSeconds(0.25f);
        m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
        yield return new WaitForSeconds(1f);

        StopAllCoroutines();
        Destroy(gameObject);
    }

    void FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDistance = Mathf.Infinity;
        GameObject closestEnemy = null;

        foreach (GameObject potentialEnemy in enemies)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, potentialEnemy.transform.position);
            if (distanceToEnemy < closestDistance)
            {
                closestDistance = distanceToEnemy;
                closestEnemy = potentialEnemy;
            }
        }

        if (closestEnemy != null && closestDistance <= rangeDist)
        {
            enemy = closestEnemy;
        }
        else
        {
            enemy = null;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            if (enemy != null || player != null)
            {
                Jump();
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Dessine les hitboxes d'attaque
        Gizmos.color = Color.green;
        foreach (Transform hitbox in attackHitboxes)
        {
            Gizmos.DrawWireSphere(hitbox.position, attackRange);
        }
    }
}
