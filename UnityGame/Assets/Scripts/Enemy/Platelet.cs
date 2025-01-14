using UnityEngine;
using System.Collections;

public class Platelet : BaseEnemy
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f; // Movement speed
    [SerializeField] private float obstacleAvoidanceRange = 1f; // Distance to detect obstacles
    [SerializeField] private LayerMask obstacleLayer; // Layer for obstacles
    private Vector2 moveDirection; // The movement direction of the platelet

    [Header("Clot Settings")]
    public GameObject clotPrefab; // Clot prefab to be spawned
    [SerializeField] private float clotSpawnInterval = 2f; // How often to spawn clot (seconds)
    private float clotSpawnTimer;

    [Header("Movement Control")]
    [SerializeField] private float directionChangeInterval = 3f; // How often to change movement direction (seconds)
    private float directionChangeTimer;

    private Rigidbody2D rb;
    private Animator animator; // Reference to the Animator

    protected override void Start()
    {
        base.Start();

        // Setup Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Get Animator component
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("Animator not found! Please attach an Animator to the Platelet.");
        }

        // Set initial random movement direction
        moveDirection = GetRandomDirection();
        clotSpawnTimer = clotSpawnInterval;
        directionChangeTimer = directionChangeInterval;
    }

    private void FixedUpdate()
    {
        Move();
        UpdateAnimator();
    }

    protected override void Move()
    {
        if (rb == null) return;

        // Check if direction needs to change
        directionChangeTimer -= Time.fixedDeltaTime;
        if (directionChangeTimer <= 0f)
        {
            moveDirection = GetRandomDirection();
            directionChangeTimer = directionChangeInterval;
        }

        // Move the Platelet in the current direction
        rb.linearVelocity = moveDirection * speed;

        // Obstacle avoidance
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, obstacleAvoidanceRange, obstacleLayer);
        if (hit.collider != null)
        {
            // Calculate a new movement direction based on the collision normal
            Vector2 collisionNormal = hit.normal;
            moveDirection = Vector2.Reflect(moveDirection, collisionNormal).normalized;
        }

        // Clamp velocity
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, speed);

        // Drop clots at intervals
        clotSpawnTimer -= Time.fixedDeltaTime;
        if (clotSpawnTimer <= 0f)
        {
            SpawnClot();
            clotSpawnTimer = clotSpawnInterval;
        }
    }

    /// <summary>
    /// Updates the Animator's Moving property based on the current velocity.
    /// </summary>
    private void UpdateAnimator()
    {
        if (animator != null)
        {
            bool isMoving = rb.linearVelocity.magnitude > 0.1f; // Moving if velocity is above a threshold
            animator.SetBool("Moving", isMoving);
        }
    }

    /// <summary>
    /// Spawns a clot (wall) at the Platelet's current position.
    /// </summary>
    private void SpawnClot()
    {
        if (clotPrefab == null) return;
        Instantiate(clotPrefab, transform.position, Quaternion.identity);
    }

    /// <summary>
    /// Get a random movement direction for the Platelet.
    /// </summary>
    /// <returns>Random Vector2 direction</returns>
    private Vector2 GetRandomDirection()
    {
        Vector2[] possibleDirections = { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
        int randomIndex = Random.Range(0, possibleDirections.Length);
        return possibleDirections[randomIndex];
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Vector2 collisionNormal = collision.contacts[0].normal;
            moveDirection = Vector2.Reflect(moveDirection, collisionNormal).normalized;
            //Debug.Log($"{gameObject.name} collided with {collision.gameObject.name} and changed direction.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw ray to visualize obstacle avoidance
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)moveDirection * obstacleAvoidanceRange);
    }
}
