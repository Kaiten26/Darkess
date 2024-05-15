using UnityEngine;

public class PlayerMovement1 : MonoBehaviour
{
    public float speed = 5.0f;
    public float jumpForce = 10.0f;
    public float dashSpeed = 20.0f;
    public float dashDuration = 0.5f;
    public Transform groundCheck;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool canDoubleJump = true;
    private bool isDashing = false;
    private bool facingRight = true;
    private float dashTimeLeft;
    private Vector2 dashDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Mouvement horizontal
        float horizontalInput = Input.GetAxis("Horizontal");
        if (!isDashing)
        {
            rb.velocity = new Vector2(horizontalInput * speed, rb.velocity.y);
            if (horizontalInput > 0 && !facingRight)
            {
                Flip();
            }
            else if (horizontalInput < 0 && facingRight)
            {
                Flip();
            }
        }

        // Vérifier si le personnage est au sol
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        // Réinitialiser le double saut quand on touche le sol
        if (isGrounded)
        {
            canDoubleJump = true;
        }

        // Saut
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            }
            else if (canDoubleJump)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0); // Réinitialiser la vitesse y
                rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
                canDoubleJump = false;
            }
        }

        // Dash
        if (Input.GetButtonDown("Fire3") && !isDashing) // 'Fire3' est généralement attribué à la touche Shift ou un bouton d'épaule
        {
            isDashing = true;
            dashTimeLeft = dashDuration;
            dashDirection = new Vector2(facingRight ? 1 : -1, 0);
        }

        if (isDashing)
        {
            rb.velocity = dashDirection * dashSpeed;
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0)
            {
                isDashing = false;
            }
        }
    }

    private void Flip()
    {
        // Inverser la direction du personnage
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}


