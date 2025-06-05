using Unity.Netcode;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour
{
    [SerializeField] private Camera mainCamera; // Reference to the main camera in prefab
    private Vector3 mousePos;
    [SerializeField] private Transform core; // Reference to the core of the player, used for rotation
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform pointOfFire; // Point from which the bullet is fired
    private bool canFire = true;
    private float timer;
    [SerializeField] private float fireRate = 0.2f; // Time in seconds between shots
    [SerializeField] Transform healthUI;

    void Start()
    {

    }

    void Update()
    {
        // On verifie si le joueur est le joueur local, si non on ne fait rien
        if (!IsOwner) return;

        // Get the mouse position in world space
        UpdateMousePos();

        // Rotate the sprite to face the mouse position
        RotateSpriteToMouseServerRpc(mousePos - transform.position);

        HandleShooting();
    }

    /// <summary>
    /// Find the main camera in the scene.
    /// If the camera is not found, it logs a warning.
    /// </summary>
    private void FindMainCamera()
    {
        GameObject cameraObject = GameObject.FindGameObjectWithTag("MainCamera");
        if (cameraObject != null)
        {
            mainCamera = cameraObject.GetComponent<Camera>();
        }
        else
        {
            Debug.LogWarning("Main Camera not found. Ensure it has the 'MainCamera' tag.");
        }
    }

    /// <summary>
    /// Server rotates the sprite to face the mouse position.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void RotateSpriteToMouseServerRpc(Vector3 rotation)
    {
        float rotZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
        core.transform.rotation = Quaternion.Euler(0f, 0f, rotZ - 90f); // Adjusting for the core sprite's orientation
    }

    /// <summary>
    /// Update the mouse position in world space.
    /// </summary>
    private void UpdateMousePos()
    {
        mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
    }

    /// <summary>
    /// Handle shooting logic when the left mouse button is pressed.
    /// </summary>
    private void HandleShooting()
    {
        if (!canFire)
        {
            timer += Time.deltaTime;
            if (timer > fireRate)
            {
                canFire = true; // Reset the ability to fire after the cooldown
                timer = 0f; // Reset the timer
            }
        }

        if (Input.GetMouseButton(0) && canFire) // Left mouse button
        {
            canFire = false; // Prevent further shooting until the cooldown is over
            Vector2 direction = (mousePos - transform.position).normalized;
            LeftShootServerAuth(direction);
        }

        else if (Input.GetMouseButton(1)) // Right mouse button
        {
            //RightShoot();
        }
    }

    private void LeftShootServerAuth(Vector2 direction)
    {
        Debug.Log("Left mouse button clicked. Shooting bullet.");

        LeftShootServerRpc(pointOfFire.position, direction, OwnerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void LeftShootServerRpc(Vector3 position, Vector2 direction, ulong ownerClientId)
    {
        Debug.Log($"Server is shooting bullet. OwnerClientId: {ownerClientId}");

        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity); // position = point of fire position
        BulletNetwork bulletNetwork = bullet.GetComponent<BulletNetwork>();

        if (bulletNetwork != null)
        {
            // Spawn the bullet on the network
            bulletNetwork.GetComponent<NetworkObject>().Spawn();
            // Set the bullet's direction based on the mouse position and the player's position
            bulletNetwork.SetDirection(direction);
            // Assign the shooterClientId to the bullet
            bulletNetwork.shooterClientId.Value = ownerClientId;
        }
        else
        {
            Debug.LogError("BulletNetwork component not found on the bullet prefab.");
        }
    }
}
