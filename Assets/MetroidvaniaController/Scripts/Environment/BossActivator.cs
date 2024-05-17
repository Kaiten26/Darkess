using UnityEngine;

public class BossActivator : MonoBehaviour
{
    public GameObject boss; // Référence au GameObject du boss

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            BossController bossController = boss.GetComponent<BossController>();
            if (bossController != null)
            {
                bossController.enabled = true; // Activer le script BossController
            }
        }
    }
}
