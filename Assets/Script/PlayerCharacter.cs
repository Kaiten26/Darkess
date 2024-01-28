using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public Transform feet;
    public LayerMask groundLayers;

    private Rigidbody2D rb;
    private float mx;
    private bool isGrounded;
    private bool isDashing;
    private float dashTimeLeft;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        mx = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartDash();
        }

        if (isDashing)
        {
            ContinueDash();
        }
    }


    void Jump()
    {
        Vector2 jumpVector = new Vector2(rb.velocity.x, jumpForce);
        rb.velocity = jumpVector;
    }

    void FixedUpdate()
    {
        if (!isDashing)
        {
            Vector2 movement = new Vector2(mx * moveSpeed, rb.velocity.y);
            rb.velocity = movement;
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        rb.velocity = new Vector2(mx * dashSpeed, rb.velocity.y);
    }

    void ContinueDash()
    {
        if (dashTimeLeft > 0)
        {
            rb.velocity = new Vector2(mx * dashSpeed, rb.velocity.y);
            dashTimeLeft -= Time.deltaTime;
        }
        else
        {
            isDashing = false;
        }
    }
}
