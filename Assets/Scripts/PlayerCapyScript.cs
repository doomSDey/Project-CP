using UnityEngine;
using UnityEngine.SceneManagement; // To reload the scene
using UnityEngine.Tilemaps; // For working with the Tilemap

public class PlayerCapyScript : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Shooter shooter; // Reference to the shooter component

    // Reference to the tilemap for boundary calculations
    public Tilemap tilemap;

    private Vector3 minBounds;
    private Vector3 maxBounds;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        shooter = GetComponentInChildren<Shooter>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0; // No gravity for the player

        // **Calculate the tilemap bounds**
        if (tilemap != null)
        {
            BoundsInt bounds = tilemap.cellBounds;
            minBounds = tilemap.CellToWorld(bounds.min);
            maxBounds = tilemap.CellToWorld(bounds.max);
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
        // Move the player
        rb.linearVelocity = movement.normalized * moveSpeed;

        // **Clamp the player's position within the tilemap boundaries**
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);
        transform.position = clampedPosition;
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
