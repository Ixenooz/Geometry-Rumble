using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR;

public class PlayerMovementNetwork : NetworkBehaviour
{
    public float moveSpeed = 3f; // Vitesse de déplacement du joueur
    private Rigidbody2D rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // On verifie si le joueur est le joueur local, si non on ne fait rien
        if (!IsOwner) return;
        
        // Gérer le mouvement du joueur en fonction des entrées clavier
        HandleMovementServerAuth();
    }
    
    /// <summary>
    /// Récupère les inputs du joueur. Le serveur déplace ensuite le joueur.
    /// </summary>
    private void HandleMovementServerAuth()
    {
        // Récupération des inputs
        float moveX = 0f;
        float moveY = 0f;

        if (Input.GetKey(KeyCode.W)) // Avancer (haut)
        {
            moveY = 1f;
        }
        if (Input.GetKey(KeyCode.S)) // Reculer (bas)
        {
            moveY = -1f;
        }
        if (Input.GetKey(KeyCode.A)) // Gauche
        {
            moveX = -1f;
        }
        if (Input.GetKey(KeyCode.D)) // Droite
        {
            moveX = 1f;
        }
        // Normalisation du vecteur pour éviter les déplacements plus rapides en diagonale
        Vector2 movement = new Vector2(moveX, moveY).normalized;

        // Déplacement du joueur par le serveur
        HandleMovementServerRpc(movement); ;
    }

    /// <summary>
    /// Déplacement du joueur géré par le serveur.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void HandleMovementServerRpc(Vector2 movement)
    {
        // Application du mouvement au Rigidbody2D
        rb.linearVelocity = movement * moveSpeed;
    }

    /// <summary>
    /// Gère le mouvement du joueur en fonction des entrées clavier.
    /// </summary>
    private void HandleMovementClient()
    {
        // Récupération des inputs
        float moveX = 0f;
        float moveY = 0f;

        if (Input.GetKey(KeyCode.W)) // Avancer (haut)
        {
            moveY = 1f;
        }
        if (Input.GetKey(KeyCode.S)) // Reculer (bas)
        {
            moveY = -1f;
        }
        if (Input.GetKey(KeyCode.A)) // Gauche
        {
            moveX = -1f;
        }
        if (Input.GetKey(KeyCode.D)) // Droite
        {
            moveX = 1f;
        }

        // Normalisation du vecteur pour éviter les déplacements plus rapides en diagonale
        Vector2 movement = new Vector2(moveX, moveY).normalized;

        // Application du mouvement au Rigidbody2D
        rb.linearVelocity = movement * moveSpeed;
    }
}
