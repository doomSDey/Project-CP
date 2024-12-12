using UnityEngine;
using UnityEngine.SceneManagement; // To reload the scene
using UnityEngine.Tilemaps; // To reference the Tilemap

public class PlayerCapyScript : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Shooter shooter; // Reference to the shooter component

    public Tilemap tilemap; // Reference to the tilemap
    private Vector3 minBounds;
    private Vector3 maxBounds;
    private Camera mainCamera;
    private Vector3 playerSize;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        shooter = GetComponentInChildren<Shooter>();
        mainCamera = Camera.main;

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0; // No gravity for the player

        // Calculate tilemap bounds
        if (tilemap != null)
        {
            Bounds mapBounds = tilemap.localBounds;
            Vector3 tilemapWorldMin = tilemap.CellToWorld(tilemap.cellBounds.min);
            Vector3 tilemapWorldMax = tilemap.CellToWorld(tilemap.cellBounds.max);

            minBounds = new Vector3(tilemapWorldMin.x, tilemapWorldMin.y, 0);
            maxBounds = new Vector3(tilemapWorldMax.x, tilemapWorldMax.y, 0);
        }

        // Calculate player size (from Collider2D)
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            playerSize = collider.bounds.extents; // Half of width and height
        }
        else
        {
            playerSize = Vector3.zero;
        }
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        // Get input for movement (WASD keys)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        // Apply movement to the player
        rb.linearVelocity = movement.normalized * moveSpeed;

        // Get the player's collider bounds
        BoxCollider2D playerCollider = GetComponent<BoxCollider2D>();
        if (playerCollider == null) return;

        // Calculate the player's half size (width and height)
        float playerHalfWidth = playerCollider.size.x / 2 * transform.localScale.x;
        float playerHalfHeight = playerCollider.size.y / 2 * transform.localScale.y;

        // Add only the shooter's vertical offset
        float shooterOffsetY = Mathf.Abs(shooter.transform.localPosition.y) * shooter.transform.localScale.y;

        // Adjust the total height to account for the shooter's position
        float totalHeight = playerHalfHeight + shooterOffsetY;

        // Correct -Y offset by adding half the player's height
        float correctedMinY = minBounds.y + playerHalfHeight;

        // Clamp the player's position to stay fully within the map
        float clampedX = Mathf.Clamp(
            transform.position.x,
            minBounds.x + playerHalfWidth,
            maxBounds.x - playerHalfWidth
        );
        float clampedY = Mathf.Clamp(
            transform.position.y,
            correctedMinY,
            maxBounds.y - totalHeight
        );

        // Apply the clamped position
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);

        // Center the camera on the player while ensuring it stays within the map
        CenterCameraOnPlayer();
    }

    private void CenterCameraOnPlayer()
    {
        if (mainCamera == null) return;

        // Get the camera's size in world units
        float cameraHalfHeight = mainCamera.orthographicSize;
        float cameraHalfWidth = mainCamera.aspect * cameraHalfHeight;

        // Clamp the camera position to ensure it stays within the tilemap bounds
        float clampedX = Mathf.Clamp(
            transform.position.x,
            minBounds.x + cameraHalfWidth,
            maxBounds.x - cameraHalfWidth
        );
        float clampedY = Mathf.Clamp(
            transform.position.y,
            minBounds.y + cameraHalfHeight,
            maxBounds.y - cameraHalfHeight
        );

        // Center the camera on the player
        mainCamera.transform.position = new Vector3(clampedX, clampedY, mainCamera.transform.position.z);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Player collided with enemy! Game Over!");
            Die();
        }
    }

    void HandleShooting()
    {
        if (Input.GetMouseButton(0)) // Left click to shoot laser
        {
            shooter.ShootLaser();
        }

        if (Input.GetMouseButtonDown(1)) // Right click to shoot bomb
        {
            shooter.ShootBomb();
        }
    }

    public void Die()
    {
        Debug.Log("Player has died!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
