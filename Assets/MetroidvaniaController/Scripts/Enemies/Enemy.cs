using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

	public float life = 10;
    public bool isDead = false; // Variable pour vérifier si l'ennemi est mort
    private bool isPlat;
	private bool isObstacle;
	private Transform fallCheck;
	private Transform wallCheck;
	public LayerMask turnLayerMask;
	private Rigidbody2D rb;

	private bool facingRight = true;
	
	public float speed = 5f;

	public bool isInvincible = false;
	private bool isHitted = false;

	void Awake () {
		fallCheck = transform.Find("FallCheck");
		wallCheck = transform.Find("WallCheck");
		rb = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {

        // Modifiez la condition de mort
        if (life <= 0 && !isDead)
        {
            StartCoroutine(DestroyEnemy());
            isDead = true; // Marquez l'ennemi comme mort
        }

        isPlat = Physics2D.OverlapCircle(fallCheck.position, .2f, 1 << LayerMask.NameToLayer("Default"));
		isObstacle = Physics2D.OverlapCircle(wallCheck.position, .2f, turnLayerMask);

		if (!isHitted && life > 0 && Mathf.Abs(rb.velocity.y) < 0.5f)
		{
			if (isPlat && !isObstacle && !isHitted)
			{
				if (facingRight)
				{
					rb.velocity = new Vector2(-speed, rb.velocity.y);
				}
				else
				{
					rb.velocity = new Vector2(speed, rb.velocity.y);
				}
			}
			else
			{
				Flip();
			}
		}
	}

	void Flip (){
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;
		
		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	public void ApplyDamage(float damage) {
		if (!isInvincible) 
		{
			float direction = damage / Mathf.Abs(damage);
			damage = Mathf.Abs(damage);
			transform.GetComponent<Animator>().SetBool("Hit", true);
			life -= damage;
			rb.velocity = Vector2.zero;
			rb.AddForce(new Vector2(direction * 500f, 100f));
			StartCoroutine(HitTime());
		}
	}

	void OnCollisionStay2D(Collision2D collision)
	{
		if (collision.gameObject.tag == "Player" && life > 0)
		{
			collision.gameObject.GetComponent<CharacterController2D>().ApplyDamage(2f, transform.position);
		}
	}

	IEnumerator HitTime()
	{
		isHitted = true;
		isInvincible = true;
		yield return new WaitForSeconds(0.1f);
		isHitted = false;
		isInvincible = false;
	}

    IEnumerator DestroyEnemy()
    {
        GetComponent<Animator>().SetBool("IsDead", true);
        //GetComponent<CapsuleCollider2D>().enabled = false;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero; // Arrête tout mouvement horizontal et vertical
        GetComponent<Rigidbody2D>().isKinematic = true; // Change le Rigidbody en kinematic pour éviter toute réponse physique
        this.enabled = false; // Désactive le script Enemy pour arrêter d'autres mises à jour et actions
        yield return new WaitForSeconds(3f); // Ce délai peut être ajusté ou omis
    }

    public void ReviveWithDelay()
    {
        StartCoroutine(ReviveCoroutine());
    }

    private IEnumerator ReviveCoroutine()
    {
        yield return new WaitForSeconds(0f); // Attente de 0.5 seconde
        Revive();
    }

    public void Revive()
    {
        GetComponent<Rigidbody2D>().isKinematic = false; // Restaure la physique normale
        GetComponent<CapsuleCollider2D>().enabled = true; // Réactive les collisions
        GetComponent<Rigidbody2D>().velocity = Vector2.zero; // Assurez-vous qu'il n'y a pas de mouvement résiduel
        GetComponent<Animator>().SetBool("IsDead", false);
        this.enabled = true;
        life = 10;
        isDead = false;
    }

}
