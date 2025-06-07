using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class BulletNetwork : NetworkBehaviour
{
    private float speed = 7f; // Vitesse du projectile
    private Rigidbody2D rb;
    private Vector2 direction; // Direction du projectile

    // Utiliser une NetworkVariable pour shooterClientId (Assignée via PlayerNetwork)
    public NetworkVariable<ulong> shooterClientId = new NetworkVariable<ulong>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction.normalized * speed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {

        if (!IsServer)
        {
            return; // Ne pas exécuter la logique de collision si ce n'est pas le serveur
        }

        // Ignorer les collisions avec d'autres balles
        if (collision.CompareTag("Bullet"))
        {
            return;
        }

        // Si la balle touche un joueur
        if (collision.CompareTag("Player"))
        {
            // Récupérer le NetworkObject du joueur touché (dans le parent du collider)
            NetworkObject playerHit = collision.GetComponentInParent<NetworkObject>();
            Debug.Log("Collision entre balle et " + playerHit.OwnerClientId + "; shooterClientId : " + shooterClientId.Value);

            // Vérifier si le joueur touché est celui qui a tiré la balle
            if (playerHit.OwnerClientId == shooterClientId.Value)
            {
                Debug.Log("[Ignorer collision] : La balle a touché le joueur qui l'a tirée.");
                return; // Ignorer la collision si c'est le joueur qui a tiré
            }

            // Logique si la balle touche un joueur différent :
            Debug.Log("Destruction de la balle.");
            DestroyBulletServerRpc();
            UpdatePlayerHealth(collision);
        }
    }

    /// <summary>
    /// Sets the direction of the bullet.
    /// </summary>
    public void SetDirection(Vector2 direction)
    {
        this.direction = direction;
    }

    /// <summary>
    /// Destroys the bullet on the server.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void DestroyBulletServerRpc()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Updates the health of the player hit by the bullet.
    /// </summary>
    public void UpdatePlayerHealth(Collider2D collision)
    {
        if (!IsServer)
        {
            return; // Ne pas exécuter la logique de mise à jour de la santé si ce n'est pas le serveur
        }

        // Récupérer le NetworkHealthState du joueur touché
        HealthStateNetwork healthState = collision.GetComponent<HealthStateNetwork>();

        if (healthState != null)
        {
            int newHealth = healthState.HealthPoint.Value - 10;


            if (newHealth <= 0)
            {
                healthState.HealthPoint.Value = 0;
                healthState.IsDead.Value = true; // Marquer le joueur comme mort
            }
            else if (newHealth > healthState.MaxHealthPoint.Value)
            {
                Debug.LogWarning("La nouvelle santé dépasse la santé maximale. Réinitialisation à la santé maximale.");
                newHealth = healthState.MaxHealthPoint.Value; // Ne pas dépasser la santé maximale
            }
            else
            {
                healthState.HealthPoint.Value = newHealth;
            }
        }
        else
        {
            Debug.LogWarning("NetworkHealthState component not found on the player object.");
        }
    }


}