using UnityEngine;

public class Ally : MonoBehaviour {
    public Transform player; // Référence au joueur
    public float minFollowDistance = 2f; // Distance minimale de suivi
    public float maxFollowDistance = 5f; // Distance maximale de suivi
    public float speed = 5f; // Vitesse de déplacement
    public float attackRange = 2f; // Portée d'attaque
    public LayerMask enemyLayer; // Layer des ennemis
    public float attackPower = 10f; // Puissance de l'attaque
    public float repulseStrength = 500f; // Force de répulsion lors d'une attaque
    public float jumpForce = 700f; // Force du saut

    private Rigidbody2D rb;
    private bool isGrounded;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) {
            player = playerObject.transform;
        } else {
            Debug.LogError("Player object not found! Make sure it is tagged correctly.");
        }
    }

    void Update() {
        if (player != null) {
            FollowPlayer();
            FindAndAttackEnemy(); // Cette méthode doit être modifiée pour attaquer les ennemis ciblés par le joueur.
        }
    }

    void FixedUpdate() {
        CheckGroundStatus();
        if (isGrounded && ShouldJump()) {
            Jump();
        }
    }

    void FollowPlayer() {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > maxFollowDistance || distanceToPlayer < minFollowDistance) {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float moveDistance = speed * Time.deltaTime;
            if (distanceToPlayer > maxFollowDistance) {
                transform.position += directionToPlayer * moveDistance;
            } else if (distanceToPlayer < minFollowDistance) {
                transform.position -= directionToPlayer * moveDistance;
            }
        }
    }

    void CheckGroundStatus() {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
    }

    bool ShouldJump() {
        // Simple logic to determine when to jump. Customize based on game needs.
        return Vector3.Distance(transform.position, player.position) > maxFollowDistance && !isGrounded;
    }

    void Jump() {
        rb.AddForce(new Vector2(0, jumpForce));
    }

    void FindAndAttackEnemy() {
        // Implement logic to target the same enemy the player is attacking.
        // This requires some form of communication or state sharing about who the player is targeting.
    }

    void Attack(Transform enemy) {
        Vector3 attackDirection = (enemy.position - transform.position).normalized;
        rb.AddForce(attackDirection * attackPower, ForceMode2D.Impulse);
        enemy.GetComponent<Enemy>().ApplyDamage(5f);
        rb.AddForce(-attackDirection * repulseStrength * Time.deltaTime, ForceMode2D.Impulse);
    }
}
