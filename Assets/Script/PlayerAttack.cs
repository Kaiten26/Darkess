using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject hitbox; // Assignez votre GameObject hitbox ici via l'inspecteur
    public float attackDuration = 0.5f; // Dur�e de l'attaque
    private float timeSinceAttack = 0.0f;
    private bool isAttacking = false;

    void Update()
    {
        // G�rer l'entr�e pour l'attaque
        if (Input.GetKeyDown(KeyCode.F) && !isAttacking) // Utilisez la touche que vous voulez pour l'attaque
        {
            StartAttack();
        }

        // D�sactiver la hitbox apr�s la dur�e de l'attaque
        if (isAttacking)
        {
            if (timeSinceAttack > attackDuration)
            {
                hitbox.SetActive(false);
                isAttacking = false;
            }
            else
            {
                timeSinceAttack += Time.deltaTime;
            }
        }
    }

    void StartAttack()
    {
        hitbox.SetActive(true);
        timeSinceAttack = 0.0f;
        isAttacking = true;
    }
}

