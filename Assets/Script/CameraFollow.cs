using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // La r�f�rence au Transform du joueur
    public float smoothSpeed = 0.125f; // Vitesse de lissage du mouvement de la cam�ra
    public Vector3 offset; // D�calage de la cam�ra par rapport au joueur

    void FixedUpdate()
    {
        Vector3 desiredPosition = player.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Pour verrouiller la cam�ra sur l'axe Z (si votre jeu est en 2D)
        transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }
}

