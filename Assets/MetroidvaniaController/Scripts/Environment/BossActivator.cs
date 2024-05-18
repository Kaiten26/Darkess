using UnityEngine;
using System.Collections.Generic;

public class BossActivator : MonoBehaviour
{
    public GameObject boss; // R�f�rence au GameObject du boss
    public List<GameObject> enemySpawners; // Liste des GameObjects des spawners d'ennemis
    public GameObject wall; // R�f�rence au GameObject du mur
    public CameraFollow cameraFollow; // R�f�rence au script CameraFollow
    public Transform newCameraTarget; // Nouvelle cible pour la cam�ra

    // Ajout pour l'audio
    public AudioSource audioSource; // R�f�rence au composant AudioSource
    public AudioClip bossMusic; // R�f�rence � la musique du boss

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            BossController bossController = boss.GetComponent<BossController>();
            if (bossController != null)
            {
                bossController.enabled = true; // Activer le script BossController
            }

            foreach (GameObject spawner in enemySpawners)
            {
                EnemySpawner enemySpawner = spawner.GetComponent<EnemySpawner>();
                if (enemySpawner != null)
                {
                    enemySpawner.enabled = true; // Activer le script EnemySpawner
                }
            }
            //no doppler effects for music
            GetComponent<AudioSource>().dopplerLevel = 0.0f;

            //no spatial blend either
            GetComponent<AudioSource>().spatialBlend = 0.0f;

            // Activer le mur et la collision
            if (wall != null)
            {
                ActivateWall();
            }

            // Changer la cible de la cam�ra
            if (cameraFollow != null && newCameraTarget != null)
            {
                cameraFollow.Target = newCameraTarget;
            }

            // Jouer la musique du boss
            if (audioSource != null && bossMusic != null)
            {
                audioSource.clip = bossMusic;
                audioSource.Play();
            }
        }
    }

    void ActivateWall()
    {
        SpriteRenderer wallSpriteRenderer = wall.GetComponent<SpriteRenderer>();
        Collider2D wallCollider = wall.GetComponent<Collider2D>();

        if (wallCollider != null)
        {
            wallCollider.enabled = true; // Activer le collider du mur
        }

        if (wallSpriteRenderer != null)
        {
            wallSpriteRenderer.enabled = true; // Activer le SpriteRenderer du mur
        }
    }
}
