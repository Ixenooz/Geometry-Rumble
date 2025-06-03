using Unity.Netcode;
using UnityEngine;

public class BulletNetwork : NetworkBehaviour
{
    private Vector3 mousePos; // Position de la souris
    private Camera mainCamera; // Référence à la caméra principale
    private float speed = 7f; // Vitesse du projectile
    private Rigidbody2D rb;
    private Vector2 direction; // Direction du projectile

    // Utiliser une NetworkVariable pour shooterClientId (Assignée via PlayerNetwork)
    public NetworkVariable<ulong> shooterClientId = new NetworkVariable<ulong>();

    void Start()
    {
        mainCamera = Camera.main.GetComponent<Camera>();
        rb = GetComponent<Rigidbody2D>();
        mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        rb.linearVelocity = direction.normalized * speed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignorer les collisions avec d'autres balles
        if (collision.CompareTag("Bullet"))
        {
            return;
        }

        // Si la balle touche un joueur
        if (collision.CompareTag("Player"))
        {
            // Récupérer le NetworkObject du joueur touché
            NetworkObject playerHit = collision.GetComponent<NetworkObject>();
            Debug.Log("Collision entre balle et " + playerHit.OwnerClientId + "; shooterClientId : " + shooterClientId.Value);

            // Vérifier si le joueur touché est celui qui a tiré la balle
            if (playerHit.OwnerClientId == shooterClientId.Value)
            {
                Debug.Log("[Ignorer collision] : La balle a touché le joueur qui l'a tirée.");
                return; // Ignorer la collision si c'est le joueur qui a tiré
            }

            // Logique si la balle touche un joueur différent :

            
        }
    }

    /// <summary>
    /// Sets the direction of the bullet.
    /// </summary>
    public void SetDirection(Vector2 direction)
    {
        this.direction = direction;
    }

    
}