using Unity.Netcode;
using UnityEngine;

public class BulletNetwork : NetworkBehaviour
{
    private float speed = 7f; // Vitesse du projectile
    private Vector2 direction = Vector2.right; // Direction du projectile (par défaut vers la droite)
    private Rigidbody2D rb;

    // Utiliser une NetworkVariable pour shooterClientId (Assignée via PlayerNetwork)
    public NetworkVariable<ulong> shooterClientId = new NetworkVariable<ulong>();

    void Start()
    {
        // Récupère le composant Rigidbody2D attaché à l'objet
        rb = GetComponent<Rigidbody2D>();

        // Définit la vitesse du projectile dès le début
        rb.linearVelocity = direction.normalized * speed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignorer les collisions avec d'autres balles
        if (collision.CompareTag("Bullet"))
        {
            return;
        }

        // Vérifier si la collision est avec un joueur
        if (collision.CompareTag("Player"))
        {
            // Récupérer le NetworkObject du joueur touché
            NetworkObject playerHit = collision.GetComponent<NetworkObject>();
            Debug.Log("Collision entre balle et " + playerHit.OwnerClientId + "; shooterClientId : " + shooterClientId.Value);

            // Vérifier si le joueur touché est celui qui a tiré la balle
            if (playerHit.OwnerClientId == shooterClientId.Value | shooterClientId.Value == 0 & !IsHost)
            {
                Debug.Log("Ignore la collision");
                // Ignorer la collision si c'est le joueur qui a tiré la balle
                return;
            }

            // Envoyer un message au serveur avec les IDs des joueurs concernés
            if (IsServer)
            {
                Debug.Log("HandlePlayerHit : " + playerHit.OwnerClientId + ", " + shooterClientId.Value);
                HandlePlayerHit(playerHit.OwnerClientId, shooterClientId.Value);
            }
            else if (IsClient)
            {
                Debug.Log("ReportPlayerHitRpc : " + playerHit.OwnerClientId + ", " + shooterClientId.Value);
                // Si la balle est sur un client, envoyer une demande au serveur
                ReportPlayerHitServerRpc(playerHit.OwnerClientId, shooterClientId.Value);
            }

            // Détruire la balle
            if (IsServer)
            {
                Debug.Log("Balle détruite");
                Destroy(gameObject);
            }
        }
        else
        {
            // Détruire la balle si elle touche autre chose (murs, etc.)
            if (IsServer)
            {
                Debug.Log("Balle détruite");
                Destroy(gameObject);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReportPlayerHitServerRpc(ulong playerHitId, ulong shooterId)
    {
        HandlePlayerHit(playerHitId, shooterId);
    }

    private void HandlePlayerHit(ulong playerHitId, ulong shooterId)
    {
        PlayerNetwork playerHitNetwork = FindPlayerNetwork(playerHitId); // Script PlayerNetwork du joueur touché
        PlayerNetwork shooterNetwork = FindPlayerNetwork(shooterId); // Script PlayerNetwork du joueur qui a tiré

        // Logique pour gérer le joueur touché
        Debug.Log($"Player {playerHitId} was hit by a bullet from Player {shooterId}");

        // Ici ajouter des effets comme réduire la santé du joueur, etc.
        if (playerHitNetwork != null)
        {
            // Appliquer les dégâts au joueur touché
            playerHitNetwork.GetComponent<Player>().takeDamage(100);
        }

        if (shooterNetwork != null)
        {
            // Augmenter les HP du joueur qui a tiré
            shooterNetwork.GetComponent<Player>().gainHP(50);
        }
    }

    private PlayerNetwork FindPlayerNetwork(ulong clientId)
    {
        // Recherche un PlayerNetwork avec le clientId spécifié
        foreach (PlayerNetwork playerNetwork in FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None))
        {
            if (playerNetwork.OwnerClientId == clientId)
            {
                return playerNetwork; // Retourne le PlayerNetwork si trouvé
            }
        }
        return null; // Retourne null si aucun PlayerNetwork n'est trouvé avec le clientId spécifié
    }
}