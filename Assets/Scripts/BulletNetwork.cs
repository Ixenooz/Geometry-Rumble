using Unity.Netcode;
using UnityEngine;

public class BulletNetwork : NetworkBehaviour
{
    private float speed = 7f; // Vitesse du projectile
    private Vector2 direction = Vector2.right; // Direction du projectile (par défaut vers la droite)
    private Rigidbody2D rb;

    void Start()
    {
        // Récupère le composant Rigidbody2D attaché à l'objet
        rb = GetComponent<Rigidbody2D>();

        // Définit la vitesse du projectile dès le début
        rb.linearVelocity = direction.normalized * speed;
    }
}