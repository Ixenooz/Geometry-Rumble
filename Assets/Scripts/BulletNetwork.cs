using Unity.Netcode;
using UnityEngine;

public class BulletNetwork : NetworkBehaviour
{
    private Vector3 mousePos; // Position de la souris
    private Camera mainCamera; // Référence à la caméra principale
    private float speed = 7f; // Vitesse du projectile
    private Rigidbody2D rb;
    private Vector2 direction; // Direction du projectile

    void Start()
    {
        mainCamera = Camera.main.GetComponent<Camera>();
        rb = GetComponent<Rigidbody2D>();
        mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        rb.linearVelocity = direction.normalized * speed;
    }

    /// <summary>
    /// Sets the direction of the bullet.
    /// </summary>
    public void SetDirection(Vector2 direction)
    {
        this.direction = direction;
    }
}