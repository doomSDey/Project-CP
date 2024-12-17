using UnityEngine;
using UnityEngine.Tilemaps;

public class MucusBlob : BaseEnemy
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 1f; // Slow movement speed
    [SerializeField] private float directionChangeInterval = 5f; // Change direction every few seconds
    private Vector2 movementDirection;
    private float directionChangeTimer;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 20; // 2 Capy Lazer hits (10 damage each) or 1 Capy Bomb (20 damage)
    private int currentHealth;

    [Header("Bounds Settings")]
    private Vector3 minBounds;
    private Vector3 maxBounds;
    private float halfWidth;
    private float halfHeight;

    private Rigidbody2D rb;

    protected override void Start()
    {
        base.Start();
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        // Set the Rigidbody to be Kinematic
        rb.bodyType = RigidbodyType2D.Kinematic;

        movementDirection = GetRandomDirection();
        directionChangeTimer = directionChangeInterval;

        // Get bounds from the tilemap
        Tilemap tilemap = FindObjectOfType<Tilemap>();
        if (tilemap != null)
        {
            Bounds mapBounds = tilemap.localBounds;
            minBounds = mapBounds.min;
            maxBounds = mapBounds.max;
        }

        // Calculate the object's size for bounds clamping
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Vector3 objectSize = spriteRenderer.bounds.size;
            halfWidth = objectSize.x / 2;
            halfHeight = objectSize.y / 2;
        }
    }

    protected override void Move()
    {
        if (rb == null) return;

        // Move the mucus blob in the current movement direction
        Vector2 newPosition = rb.position + movementDirection * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        // Check if the blob collides with screen bounds and reflect if necessary
        CheckBounds();

        // Update the timer to change direction periodically
        directionChangeTimer -= Time.deltaTime;
        if (directionChangeTimer <= 0f)
        {
            movementDirection = GetRandomDirection();
            directionChangeTimer = directionChangeInterval;
        }
    }

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

    private Vector2 GetRandomDirection()
    {
        Vector2[] possibleDirections = { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
        int randomIndex = Random.Range(0, possibleDirections.Length);
        return possibleDirections[randomIndex];
    }

    public override void TakeDamage(float damage)
    {
        currentHealth -= (int)damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log($"{gameObject.name} has died!");
        Destroy(gameObject);
    }

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
            // Reflect direction on collision with another object
            Vector2 collisionNormal = collision.contacts[0].normal;
            movementDirection = Vector2.Reflect(movementDirection, collisionNormal).normalized;
            Debug.Log($"{gameObject.name} collided with {collision.gameObject.name} and bounced off.");
        }
    }

    private void PushPlayerBack(Rigidbody2D playerRb, Vector2 collisionNormal)
    {
        // Calculate push-back direction (opposite of collision normal)
        Vector2 pushDirection = collisionNormal.normalized;
        float pushForce = 5f; // How much force to push the player back with
        playerRb.AddForce(pushDirection * pushForce, ForceMode2D.Impulse);
    }
}
