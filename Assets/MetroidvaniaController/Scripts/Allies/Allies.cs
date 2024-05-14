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

    public GameObject throwableObject;

    private float randomDecision = 0;
    private bool doOnceDecision = true;
    private bool endDecision = false;
    private Animator anim;

    void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        attackCheck = transform.Find("AttackCheck").transform;
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player"); // Assurez-vous que le joueur a le tag "Player"

        // Lancer la coroutine pour détruire l'allié après 5 secondes
        StartCoroutine(LifeSpan(5f));
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
                        MeleeAttack();
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

                        if (randomDecision < 0.4f)
                            Run();
                        else if (randomDecision >= 0.4f && randomDecision < 0.6f)
                            Jump();
                        else if (randomDecision >= 0.6f && randomDecision < 0.8f)
                            StartCoroutine(Dash());
                        else if (randomDecision >= 0.8f && randomDecision < 0.95f)
                            RangeAttack();
                        else
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
        // Otherwise if the input is moving the ally left and the ally is facing right...
        else if (transform.localScale.x * m_Rigidbody2D.velocity.x < 0 && m_FacingRight && life > 0)
        {
            // ... flip the ally.
            Flip();
        }
    }

    void Flip()
    {
        // Switch the way the ally is labelled as facing.
        facingRight = !facingRight;

        // Multiply the ally's x local scale by -1.
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
            anim.SetBool("Hit", true);
            life -= damage;
            transform.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
            transform.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(direction * 300f, 100f));
            StartCoroutine(HitTime());
        }
    }

    public void MeleeAttack()
    {
        transform.GetComponent<Animator>().SetBool("Attack", true);
        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, 0.9f);
        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Enemy" && collidersEnemies[i].gameObject != gameObject)
            {
                if (transform.localScale.x < 1)
                {
                    dmgValue = -dmgValue;
                }
                collidersEnemies[i].gameObject.SendMessage("ApplyDamage", dmgValue);
            }
        }
        StartCoroutine(WaitToAttack(0.5f));
    }

    public void RangeAttack()
    {
        if (doOnceDecision)
        {
            GameObject throwableProj = Instantiate(throwableObject, transform.position + new Vector3(transform.localScale.x * 0.5f, -0.2f), Quaternion.identity) as GameObject;
            throwableProj.GetComponent<ThrowableProjectile>().owner = gameObject;
            Vector2 direction = new Vector2(transform.localScale.x, 0f);
            throwableProj.GetComponent<ThrowableProjectile>().direction = direction;
            StartCoroutine(NextDecision(0.5f));
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
        anim.SetBool("IsWaiting", true);
    }

    public void FollowTarget(GameObject target)
    {
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
        anim.SetBool("IsDashing", true);
        isDashing = true;
        yield return new WaitForSeconds(0.1f);
        isDashing = false;
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
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(1f, 0.25f);
        capsule.offset = new Vector2(0f, -0.8f);
        capsule.direction = CapsuleDirection2D.Horizontal;
        transform.GetComponent<Animator>().SetBool("IsDead", true);
        yield return new WaitForSeconds(0.25f);
        m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
        yield return new WaitForSeconds(1f);

        Destroy(gameObject);
    }

    IEnumerator LifeSpan(float duration)
    {
        yield return new WaitForSeconds(duration);
        StartCoroutine(DestroyAlly());
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
}
