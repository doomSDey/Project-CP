using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class PlayerCapyScript : MonoBehaviour
{
    public float moveSpeed = 5f;
    private float currentMoveSpeed;  // Added to track current speed
    private Rigidbody2D rb;
    private Vector2 movement;
    private Shooter shooter;

    public Tilemap tilemap;
    private Vector3 minBounds;
    private Vector3 maxBounds;
    private Camera mainCamera;
    private Vector3 playerSize;

    // Add bomb cooldown variables
    public float bombCooldown = 1.5f;
    private float bombCooldownTimer = 0f;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        shooter = GetComponentInChildren<Shooter>();
        mainCamera = Camera.main;
        currentMoveSpeed = moveSpeed;  // Initialize current speed

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0;

        if (tilemap != null)
        {
            Bounds mapBounds = tilemap.localBounds;
            Vector3 tilemapWorldMin = tilemap.CellToWorld(tilemap.cellBounds.min);
            Vector3 tilemapWorldMax = tilemap.CellToWorld(tilemap.cellBounds.max);

            minBounds = new Vector3(tilemapWorldMin.x, tilemapWorldMin.y, 0);
            maxBounds = new Vector3(tilemapWorldMax.x, tilemapWorldMax.y, 0);
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            playerSize = collider.bounds.extents;
        }
        else
        {
            playerSize = Vector3.zero;
        }
    }

    void Update()
    {
        if (bombCooldownTimer > 0)
        {
            bombCooldownTimer -= Time.deltaTime;
        }

        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movement.normalized * currentMoveSpeed;  // Using currentMoveSpeed instead of moveSpeed

        BoxCollider2D playerCollider = GetComponent<BoxCollider2D>();
        if (playerCollider == null) return;

        float playerHalfWidth = playerCollider.size.x / 2 * transform.localScale.x;
        float playerHalfHeight = playerCollider.size.y / 2 * transform.localScale.y;

        float shooterOffsetY = Mathf.Abs(shooter.transform.localPosition.y) * shooter.transform.localScale.y;

        float totalHeight = playerHalfHeight + shooterOffsetY;

        float correctedMinY = minBounds.y + playerHalfHeight;

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

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);

        CenterCameraOnPlayer();
    }

    private void CenterCameraOnPlayer()
    {
        if (mainCamera == null) return;

        float cameraHalfHeight = mainCamera.orthographicSize;
        float cameraHalfWidth = mainCamera.aspect * cameraHalfHeight;

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
            currentMoveSpeed = moveSpeed * 0.6f;  // Reduce speed by 40%
        }
        else if (Input.GetMouseButton(1) && bombCooldownTimer <= 0) // Right click to shoot bomb with cooldown check
        {
            shooter.ShootBomb();
            currentMoveSpeed = 0f;  // Reduce speed by 100%
            bombCooldownTimer = bombCooldown; // Reset the cooldown timer
        }
        else if (Input.GetMouseButton(1)) // Still holding right click but in cooldown
        {
            currentMoveSpeed = 0f;  // Keep speed at 0 while holding right click
        }
        else
        {
            currentMoveSpeed = moveSpeed;  // Reset to normal speed when not shooting
        }
    }

    public void Die()
    {
        Debug.Log("Player has died!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
