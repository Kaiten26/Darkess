using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public float dmgValue = 4;
    public GameObject throwableObject;
    public Transform attackCheck;
    private Rigidbody2D m_Rigidbody2D;
    public Animator animator;
    public bool canAttack = true;

    public GameObject cam;

    private BoxCollider2D attackHitbox;

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        attackHitbox = GetComponent<BoxCollider2D>();
        attackHitbox.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X) && canAttack)
        {
            canAttack = false;
            animator.SetBool("IsAttacking", true);
            StartCoroutine(AttackCooldown());
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            GameObject throwableWeapon = Instantiate(throwableObject, transform.position + new Vector3(transform.localScale.x * 0.5f, -0.2f), Quaternion.identity) as GameObject;
            Vector2 direction = new Vector2(transform.localScale.x, 0);
            throwableWeapon.GetComponent<ThrowableWeapon>().direction = direction;
            throwableWeapon.name = "ThrowableWeapon";
        }
    }

    IEnumerator AttackCooldown()
    {
        ActivateHitbox();
        yield return new WaitForSeconds(0.25f);
        DeactivateHitbox();
        canAttack = true;
    }

    private void ActivateHitbox()
    {
        attackHitbox.enabled = true;
    }

    private void DeactivateHitbox()
    {
        attackHitbox.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            collision.gameObject.SendMessage("ApplyDamage", dmgValue);
            cam.GetComponent<CameraFollow>().ShakeCamera();
        }
        else if (collision.gameObject.tag == "Boss")
        {
            collision.gameObject.SendMessage("ApplyDamage", dmgValue);
            cam.GetComponent<CameraFollow>().ShakeCamera();
        }
    }
}
