using System.Collections;
using UnityEngine;

public class SoulScript : MonoBehaviour
{
    public GameObject allyMonsterPrefab; // Le prefab du monstre alli�
    public float interactionRange = 2.0f; // Distance � laquelle le joueur peut interagir avec l'�me

    private GameObject player; // Pour stocker une r�f�rence au joueur
    private bool isSpawning = false; // Pour emp�cher les spawns multiples

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player"); // Assurez-vous que le joueur a le tag "Player"
    }

    void Update()
    {
        if (isSpawning) return; // Si d�j� en train de spawn, sortir

        // V�rifier si le joueur est � port�e et si le joueur appuie sur 'E'
        if (Vector3.Distance(player.transform.position, transform.position) <= interactionRange && Input.GetKeyDown(KeyCode.E))
        {
            isSpawning = true; // Emp�cher les spawns multiples
            StartCoroutine(SpawnAllyAfterDelay(1)); // Appel de la coroutine avec un d�lai
        }
    }

    IEnumerator SpawnAllyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Attendre pendant le d�lai sp�cifi�
        Instantiate(allyMonsterPrefab, transform.position, Quaternion.identity); // Instancier le monstre alli� � la position de l'�me
        Destroy(gameObject); // Optionnel : d�truire l'entit� �me apr�s l'interaction
    }
}
