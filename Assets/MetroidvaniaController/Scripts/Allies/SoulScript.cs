using System.Collections;
using UnityEngine;

public class SoulScript : MonoBehaviour
{
    public GameObject allyMonsterPrefab; // Le prefab du monstre allié
    public float interactionRange = 2.0f; // Distance à laquelle le joueur peut interagir avec l'âme

    private GameObject player; // Pour stocker une référence au joueur
    private bool isSpawning = false; // Pour empêcher les spawns multiples

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player"); // Assurez-vous que le joueur a le tag "Player"
    }

    void Update()
    {
        if (isSpawning) return; // Si déjà en train de spawn, sortir

        // Vérifier si le joueur est à portée et si le joueur appuie sur 'E'
        if (Vector3.Distance(player.transform.position, transform.position) <= interactionRange && Input.GetKeyDown(KeyCode.E))
        {
            isSpawning = true; // Empêcher les spawns multiples
            StartCoroutine(SpawnAllyAfterDelay(1)); // Appel de la coroutine avec un délai
        }
    }

    IEnumerator SpawnAllyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Attendre pendant le délai spécifié
        Instantiate(allyMonsterPrefab, transform.position, Quaternion.identity); // Instancier le monstre allié à la position de l'âme
        Destroy(gameObject); // Optionnel : détruire l'entité âme après l'interaction
    }
}
