using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

	public CharacterController2D controller;
	public Animator animator;

	public float runSpeed = 40f;

	float horizontalMove = 0f;
	bool jump = false;
	bool dash = false;

	//bool dashAxis = false;
	
	// Update is called once per frame
	void Update () {

		horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

		animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

		if (Input.GetKeyDown(KeyCode.Z))
		{
			jump = true;
		}

		if (Input.GetKeyDown(KeyCode.C))
		{
			dash = true;
		}

        if (Input.GetKeyDown(KeyCode.E))
        {
            CheckAndReviveEnemies();
        }

        /*if (Input.GetAxisRaw("Dash") == 1 || Input.GetAxisRaw("Dash") == -1) //RT in Unity 2017 = -1, RT in Unity 2019 = 1
		{
			if (dashAxis == false)
			{
				dashAxis = true;
				dash = true;
			}
		}
		else
		{
			dashAxis = false;
		}
		*/

    }

    void CheckAndReviveEnemies()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 3f);
        Debug.Log("Checking for dead enemies to revive...");
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider != null && hitCollider.gameObject.tag == "Enemy")
            {
                Enemy enemyScript = hitCollider.GetComponent<Enemy>();
                Ally AllyScript = hitCollider.GetComponent<Ally>();

                if (enemyScript != null && enemyScript.isDead)
                {
                    Debug.Log("Reviving enemy: " + hitCollider.gameObject.name);
                    enemyScript.ReviveWithDelay();
                }

                else if (AllyScript != null && AllyScript.isDead)
                {
                    Debug.Log("Reviving enemy: " + hitCollider.gameObject.name);
                    AllyScript.ReviveWithDelay();
                }
            }
        }
    }




    public void OnFall()
	{
		animator.SetBool("IsJumping", true);
	}

	public void OnLanding()
	{
		animator.SetBool("IsJumping", false);
	}

	void FixedUpdate ()
	{
		// Move our character
		controller.Move(horizontalMove * Time.fixedDeltaTime, jump, dash);
		jump = false;
		dash = false;
	}
}
