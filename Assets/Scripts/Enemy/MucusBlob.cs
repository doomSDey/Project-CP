using UnityEngine;
using UnityEngine.Tilemaps;

public class MucusBlob : BaseEnemy
{
    [Header("Movement Settings")]
    [SerializeField] private float changeDirectionInterval = 5f; // Change direction every few seconds
    private Vector2 movementDirection;
    private float directionChangeTimer;

    [Header("Bounds Settings")]
    private Vector3 minBounds;
    private Vector3 maxBounds;
    private float halfWidth;
    private float halfHeight;

    protected override void Start()
    {
        base.Start();

        // Set specific stats for the Mucus Blob
        currentHealth = 20; // 2 Capy Lazer hits (10 each) or 1 Capy Bomb (20 damage)
        speed = 1f; // Slow movement
        movementDirection = GetRandomDirection();
        directionChangeTimer = changeDirectionInterval;

        // Get the tilemap bounds
        Tilemap tilemap = FindAnyObjectByType<Tilemap>();
        if (tilemap != null)
        {
            Bounds mapBounds = tilemap.localBounds;
            minBounds = mapBounds.min;
            maxBounds = mapBounds.max;
        }

        // Calculate the object's size for clamping
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Vector3 objectSize = spriteRenderer.bounds.size;
            halfWidth = objectSize.x / 2;
            halfHeight = objectSize.y / 2;
        }

        // Set Rigidbody to Kinematic
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    protected override void Move()
    {
        if (rb == null) return;

        // Move the Mucus Blob according to its movement direction
        Vector2 newPosition = rb.position + movementDirection * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        // Check if the blob collides with screen bounds and reflect if necessary
        CheckBounds();

        // Periodically change movement direction
        directionChangeTimer -= Time.deltaTime;
        if (directionChangeTimer <= 0f)
        {
            movementDirection = GetRandomDirection();
            directionChangeTimer = changeDirectionInterval;
        }
    }

    /// <summary>
    /// Ensures the blob stays within the defined bounds.
    /// If it touches the edge, it bounces back.
    /// </summary>
    private void CheckBounds()
    {
        Vector3 position = transform.position;

        // Check horizontal bounds
        if (position.x - halfWidth <= minBounds.x || position.x + halfWidth >= maxBounds.x)
        {
            movementDirection.x *= -1; // Reverse horizontal direction
            position.x = Mathf.Clamp(position.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
            rb.MovePosition(position);
        }

        // Check vertical bounds
        if (position.y - halfHeight <= minBounds.y || position.y + halfHeight >= maxBounds.y)
        {
            movementDirection.y *= -1; // Reverse vertical direction
            position.y = Mathf.Clamp(position.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);
            rb.MovePosition(position);
        }
    }

    /// <summary>
    /// Get a random direction for the blob to move.
    /// </summary>
    private Vector2 GetRandomDirection()
    {
        Vector2[] possibleDirections = { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
        int randomIndex = Random.Range(0, possibleDirections.Length);
        return possibleDirections[randomIndex];
    }

    /// <summary>
    /// Handles collision logic for when the MucusBlob collides with another object.
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player collided with Mucus Blob — Player is pushed back!");

            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                PushPlayerBack(playerRb, collision.contacts[0].normal);
            }
        }
        else if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Obstacle"))
        {
            HandleObstacleCollision(collision);
        }
    }

    /// <summary>
    /// Push the player away from the mucus blob.
    /// </summary>
    private void PushPlayerBack(Rigidbody2D playerRb, Vector2 collisionNormal)
    {
        // Calculate push-back direction (opposite of collision normal)
        Vector2 pushDirection = collisionNormal.normalized;
        float pushForce = 5f; // How much force to push the player back with
        playerRb.AddForce(pushDirection * pushForce, ForceMode2D.Impulse);
    }

    /// <summary>
    /// Handles collision with obstacles and changes direction on collision.
    /// </summary>
    private  void HandleObstacleCollision(Collision2D collision)
    {
        Vector2 collisionNormal = collision.GetContact(0).normal;
        movementDirection = Vector2.Reflect(movementDirection, collisionNormal).normalized;
        rb.linearVelocity = movementDirection * speed;
        Debug.Log($"{gameObject.name} bounced off {collision.gameObject.name} in direction {movementDirection}");
    }
}
