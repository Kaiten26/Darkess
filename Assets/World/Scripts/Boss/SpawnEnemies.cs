using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // L'ennemi � spawner
    public GameObject enemyPrefab;

    // Temps entre les spawns
    public float spawnInterval = 5f;

    // Nombre maximum d'ennemis � spawner
    public int maxEnemies = 10;

    // Liste des ennemis actuellement spawnes
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    public BossController bossController;

    private Coroutine spawnCoroutine;

    void Start()
    {
        // D�marrer la coroutine de spawn
        spawnCoroutine = StartCoroutine(SpawnEnemies());

        // Abonnez-vous � l'�v�nement de mort du boss
        if (bossController != null)
        {
            bossController.OnBossDeath += HandleBossDeath;
        }
    }

    IEnumerator SpawnEnemies()
    {
        while (true)
        {
            // Attendre avant de spawner le prochain ennemi
            yield return new WaitForSeconds(spawnInterval);

            // V�rifier si le nombre maximum d'ennemis n'est pas atteint
            if (spawnedEnemies.Count < maxEnemies)
            {
                // Spawner un nouvel ennemi � la position du spawner
                GameObject newEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);

                // Ajouter l'ennemi � la liste des ennemis spawnes
                spawnedEnemies.Add(newEnemy);

                // S'abonner � l'�v�nement de destruction de l'ennemi pour le retirer de la liste
                newEnemy.GetComponent<SmartEnemy>().OnDestroyEvent += () => spawnedEnemies.Remove(newEnemy);
            }
        }
    }

    private void HandleBossDeath()
    {
        // Arr�ter la coroutine de spawn
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        // D�truire tous les ennemis existants
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }

        // Vider la liste des ennemis spawnes
        spawnedEnemies.Clear();
        this.enabled= false;
    }
}
