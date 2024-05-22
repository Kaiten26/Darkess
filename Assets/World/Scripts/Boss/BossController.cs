using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    // Déclarez l'événement de mort du boss
    public event System.Action OnBossDeath;

    public float moveSpeed = 3f;
    public float attackInterval = 5f; // Intervalle entre les cycles d'attaque
    public float attackCooldown = 5f; // Cooldown entre deux attaques
    public float vanishTime = 1f;
    public Transform[] attackPoints;
    private int currentAttackPointIndex = 0;

    public Transform slamAttackPoint; // Point de position pour le slam attack
    public Transform bossDeathPoint; // Point de téléportation pour la mort du boss

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

    public GameObject ObjectScript;

    public bool IsDead { get; private set; } = false; // Ajoutez cette ligne

    // Nouvelle variable pour la distance d'arrêt
    public float stoppingDistance = 1.5f;

    // Ajout pour l'audio
    public AudioSource audioSource; // Référence au composant AudioSource
    public float fadeOutDuration = 1f; // Durée du fondu de la musique

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHP = maxHP;
        lastAttackTime = -attackCooldown; // Initialisation pour permettre une attaque immédiate
        StartCoroutine(AttackCycle());
    }

    void Update()
    {
        if (!isAttacking && !isVanishing && canMove == true)
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
        // Trouver le joueur uniquement comme cible
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (player != null && canMove == true)
        {
            // Vérifiez la distance entre le boss et le joueur
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer > stoppingDistance)
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
            else
            {
                // Arrêtez le mouvement si le boss est suffisamment proche du joueur
                rb.velocity = Vector2.zero;
                animator.SetBool("Move", false);
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
                lastAttackTime = Time.time; // Mise à jour du temps de la dernière attaque
                Debug.Log(lastAttackTime);
            }
        }
    }

    IEnumerator SliceAttack()
    {
        canMove = true; // Permet au boss de bouger pendant cette attaque
        animator.SetTrigger("Attack1");

        // Attaque à la frame 3 (0.3 secondes à 10 FPS)
        yield return new WaitForSeconds(0.3f);
        PerformAttack(sliceAttackHitboxes);

        // Attaque à la frame 10 (1.0 seconde à 10 FPS)
        yield return new WaitForSeconds(0.7f);
        PerformAttack(sliceAttackHitboxes);

        // Attendre que l'animation de 16 frames (environ 1.6 secondes) se termine
        yield return new WaitForSeconds(0.6f);

        canMove = true; // Réautorise le mouvement après l'attaque
    }

    IEnumerator SlamAttack()
    {
        canMove = false; // Empêche le boss de bouger pendant cette attaque
        rb.velocity = Vector2.zero;

        // Déclencher l'animation SlamAttack
        animator.SetTrigger("SlamAttack");
        yield return new WaitForSeconds(0.7f);

        // Téléportation au point d'attaque de slam
        if (slamAttackPoint != null)
            {
                transform.position = slamAttackPoint.position;
                Debug.Log("Boss teleported to slam attack point: " + transform.position);
            }

        // Attaque à la frame 27 (2.7 secondes à 10 FPS)
        yield return new WaitForSeconds(2.7f);
        PerformAttack(slamAttackHitboxes);

        // Attaque à la frame 31 (3.1 secondes à 10 FPS)
        yield return new WaitForSeconds(0.4f);
        PerformAttack(slamAttackHitboxes);

        // Attendre que l'animation de 39 frames (environ 3.9 secondes) se termine
        yield return new WaitForSeconds(0.8f);

        yield return new WaitForSeconds(1.3f);

        canMove = true; // Réautorise le mouvement après l'attaque
    }

    IEnumerator VanishAttack()
    {
        if (attackPoints.Length == 0)
        {
            Debug.LogError("No attack points set for VanishAttack!");
            yield break;
        }

        canMove = false; // Empêche le boss de bouger pendant cette attaque
        rb.velocity = Vector2.zero; // Stop moving
        animator.SetTrigger("Vanish");
        yield return new WaitForSeconds(0.7f);

        // Ensure the index is within the bounds of the array
        currentAttackPointIndex = currentAttackPointIndex % attackPoints.Length;
        transform.position = attackPoints[currentAttackPointIndex].position;
        Debug.Log("Boss teleported to attack point: " + transform.position);
        currentAttackPointIndex = (currentAttackPointIndex + 1) % attackPoints.Length;

        yield return new WaitForSeconds(vanishTime);

        // Réapparaît
        animator.SetTrigger("Appear");

        // Lancer l'animation de VanishAttack
        animator.SetTrigger("VanishAttack");
        yield return new WaitForSeconds(0.9f);

        // Attaque à la frame 2 (0.2 secondes à 10 FPS)
        yield return new WaitForSeconds(0.2f);
        PerformAttack(vanishAttackHitboxes);

        // Attaque à la frame 4 (0.4 secondes à 10 FPS)
        yield return new WaitForSeconds(0.2f);
        PerformAttack(vanishAttackHitboxes);

        // Attendre que l'animation de 9 frames (environ 0.9 seconde) se termine
        yield return new WaitForSeconds(0.5f);

        canMove = true; // Réautorise le mouvement après l'attaque
    }

    void PerformAttack(List<Transform> hitboxes)
    {
        foreach (Transform hitbox in hitboxes)
        {
            Collider2D[] hitTargets = Physics2D.OverlapCircleAll(hitbox.position, attackRange);
            foreach (Collider2D target in hitTargets)
            {
                if (target.tag == "Player")
                {
                    target.GetComponent<CharacterController2D>().ApplyDamage(attackDamage, transform.position);
                }
                else if (target.tag == "Allies") // Ajout pour toucher les alliés
                {
                    target.GetComponent<Allies>().ApplyDamage(attackDamage); // S'assurer que la méthode ApplyDamage existe dans le script Allies
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
        currentHP -= damage; // Réduire les HP du boss
        currentHP = Mathf.Max(currentHP, 0); // Assurez-vous que les HP ne descendent pas en dessous de 0
        Debug.Log("Boss takes " + damage + " damage. Current HP: " + currentHP);
        animator.SetTrigger("Hit");

        if (currentHP <= 0)
        {
            StartCoroutine(Die()); // Appeler la méthode Die avec une coroutine
        }
    }

    IEnumerator Die()
    {
        isVanishing = true;
        canMove = false; // Empêche le boss de bouger pendant cette attaque
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

        // Disable the Rigidbody2D
        rb.simulated = false;
        Debug.Log("Boss Rigidbody2D disabled");

        // Play death animation
        animator.SetTrigger("Die");
        Debug.Log("Boss playing death animation");

        // Désactivez le script BossController pour empêcher tout mouvement ou action supplémentaire
        StopAllCoroutines();
        this.enabled = false;

        IsDead = true; // Ajoutez cette ligne

        // Invoquer l'événement de mort du boss
        OnBossDeath?.Invoke();

        // Lancer le fondu de la musique
        if (audioSource != null)
        {
            StartCoroutine(FadeOutMusic());
        }

        isVanishing = false;
        // Le boss ne bouge plus après être mort
    }

    IEnumerator FadeOutMusic()
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeOutDuration;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume; // Reset the volume for potential future use
    }
}
