using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Utilisez Transform au lieu de GameObject pour un accès plus direct
    public float verticalOffset; // Ajout d'un offset vertical pour la caméra
    public float horizontalOffset; // Renommage de offset en horizontalOffset pour plus de clarté
    public float smoothTime = 0.5f; // Temps de lissage pour SmoothDamp

    private Vector3 velocity = Vector3.zero;

    void FixedUpdate()
    {
        // Calculez la position cible de la caméra en ajoutant l'offset horizontal basé sur la direction du joueur
        float targetHorizontalOffset = player.localScale.x > 0f ? horizontalOffset : -horizontalOffset;
        Vector3 targetPosition = new Vector3(
            player.position.x + targetHorizontalOffset,
            player.position.y + verticalOffset,
            transform.position.z
        );

        // Utilisez SmoothDamp pour déplacer la caméra de manière fluide vers la position cible
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
