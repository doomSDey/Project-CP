using UnityEngine;
using UnityEngine.Tilemaps;

public class MucusBlob : BaseEnemy
{
    private Vector2 movementDirection;
    private float changeDirectionInterval = 3f;
    private float directionChangeTimer;

    private Vector3 minBounds;
    private Vector3 maxBounds;
    private float halfWidth;
    private float halfHeight;

    protected override void Start()
    {
        base.Start();
        maxHealth = 20;
        currentHealth = maxHealth;
        speed = 1f;

        movementDirection = GetRandomDirection();
        directionChangeTimer = changeDirectionInterval;

        // Set bounds from the tilemap
        Tilemap tilemap = FindObjectOfType<Tilemap>();
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
    }

    protected override void Move()
    {
        if (rb == null) return;

        // Move the blob
        rb.linearVelocity = movementDirection * speed;

        // Check bounds and bounce back immediately
        Vector3 position = transform.position;

        // Horizontal collision
        if (position.x - halfWidth <= minBounds.x || position.x + halfWidth >= maxBounds.x)
        {
            movementDirection.x *= -1; // Reverse horizontal direction
            rb.linearVelocity = movementDirection * speed; // Update velocity immediately
            transform.position = new Vector3(
                Mathf.Clamp(position.x, minBounds.x + halfWidth, maxBounds.x - halfWidth),
                position.y,
                position.z
            );
        }

        // Vertical collision
        if (position.y - halfHeight <= minBounds.y || position.y + halfHeight >= maxBounds.y)
        {
            movementDirection.y *= -1; // Reverse vertical direction
            rb.linearVelocity = movementDirection * speed; // Update velocity immediately
            transform.position = new Vector3(
                position.x,
                Mathf.Clamp(position.y, minBounds.y + halfHeight, maxBounds.y - halfHeight),
                position.z
            );
        }

        // Update direction change timer
        directionChangeTimer -= Time.deltaTime;
        if (directionChangeTimer <= 0f)
        {
            movementDirection = GetRandomDirection();
            directionChangeTimer = changeDirectionInterval;
        }
    }

    private Vector2 GetRandomDirection()
    {
        Vector2[] possibleDirections = { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
        int randomIndex = Random.Range(0, possibleDirections.Length);
        return possibleDirections[randomIndex];
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Calculate bounce direction
            Vector2 collisionNormal = collision.GetContact(0).normal;
            movementDirection = Vector2.Reflect(movementDirection, collisionNormal).normalized;

            // Update velocity
            rb.linearVelocity = movementDirection * speed;

            Debug.Log($"{gameObject.name} bounced off {collision.gameObject.name}");
        }
    }
}
