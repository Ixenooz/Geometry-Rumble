using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    public float moveSpeed = 3f; // Vitesse de déplacement du joueur
    private Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        // Uniquement si le joueur est l'owner du client
        if (!IsOwner) return;
        
        // Déplacement du joueur
        HandleMovement();
    }

    /// <summary>
    /// Gère le mouvement du joueur en fonction des entrées clavier.
    /// </summary>
    private void HandleMovement()
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
