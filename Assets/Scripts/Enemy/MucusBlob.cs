using UnityEngine;

public class MucusBlob : BaseEnemy
{
    [Header("Movement Settings")]
    [SerializeField] private float changeDirectionInterval = 5f; // Change direction every few seconds
    private Vector2 movementDirection;
    private float directionChangeTimer;

    [Header("Physics Settings")]
    [SerializeField] private LayerMask obstacleLayer; // Layer mask for obstacles

    protected override void Start()
    {
        base.Start();

        // Set specific stats for the Mucus Blob
        currentHealth = 20; // 2 Capy Lazer hits (10 each) or 1 Capy Bomb (20 damage)
        speed = 1f; // Slow movement
        movementDirection = GetRandomDirection();
        directionChangeTimer = changeDirectionInterval;

        // Set Rigidbody to Dynamic for proper collision handling
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0; // No gravity
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent rotation
    }

    protected override void Move()
    {
        if (rb == null) return;

        // Move the Mucus Blob according to its movement direction
        Vector2 newPosition = rb.position + movementDirection * speed * Time.fixedDeltaTime;

        // Check for obstacles in the direction of movement
        RaycastHit2D hit = Physics2D.Raycast(rb.position, movementDirection, speed * Time.fixedDeltaTime, obstacleLayer);
        if (hit.collider != null)
        {
            BounceBack(hit.normal); // Reflect direction if obstacle detected
        }
        else
        {
            rb.MovePosition(newPosition);
        }

        // Periodically change movement direction
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

    private void BounceBack(Vector2 collisionNormal)
    {
        // Reflect the movement direction based on the collision normal
        movementDirection = Vector2.Reflect(movementDirection, collisionNormal).normalized;
        Debug.Log($"{gameObject.name} bounced back after hitting {collisionNormal}");
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
    }

    private void PushPlayerBack(Rigidbody2D playerRb, Vector2 collisionNormal)
    {
        Vector2 pushDirection = collisionNormal.normalized;
        float pushForce = 5f; // Push-back force
        playerRb.AddForce(pushDirection * pushForce, ForceMode2D.Impulse);
    }
}
