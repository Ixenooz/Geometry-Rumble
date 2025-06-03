using UnityEngine;

public class PlayerAiming : MonoBehaviour
{
    private Camera mainCamera;
    private Vector3 mousePos;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform pointOfFire; // Point from which the bullet is fired
    private bool canFire = true;
    private float timer;
    [SerializeField] private float fireRate = 0.2f; // Time in seconds between shots

    void Start()
    {
        // Find the main camera by tag
        FindMainCamera();
    }

    void Update()
    {
        if (mainCamera == null)
        {
            FindMainCamera(); // Try to find the camera again if it's not yet assigned
            if (mainCamera == null) return; // Exit if camera is still not found
        }

        // Get the mouse position in world space
        UpdateMousePos();

        // Rotate the sprite to face the mouse position
        RotateSpriteToMouse();

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
    /// Rotate the sprite to face the mouse position.
    /// </summary>
    private void RotateSpriteToMouse()
    {
        Vector3 rotation = mousePos - transform.position;
        float rotZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ - 90f); // Adjusting for the sprite's orientation
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
            LeftShoot();
        }

        else if (Input.GetMouseButton(1)) // Right mouse button
        {
            //RightShoot();
        }
    }

    private void LeftShoot()
    {
        Debug.Log("Left mouse button clicked. Shooting bullet.");

        GameObject bullet = Instantiate(bulletPrefab, pointOfFire.position, Quaternion.identity);
        BulletNetwork bulletNetwork = bullet.GetComponent<BulletNetwork>();

        if (bulletNetwork != null)
        {
            // Set the bullet's direction based on the mouse position and the player's position
            Vector2 direction = (mousePos - transform.position).normalized;
            bulletNetwork.SetDirection(direction);
        }
        else
        {
            Debug.LogError("BulletNetwork component not found on the bullet prefab.");
        }
    }
}
