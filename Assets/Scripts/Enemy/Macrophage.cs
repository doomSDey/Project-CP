using UnityEngine;
using System.Collections;

public class Macrophage : BaseEnemy
{
    [Header("Chase Settings")]
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] private float stunDuration = 2f;
    [SerializeField] private float obstacleAvoidanceRange = 1f;
    [SerializeField] private LayerMask obstacleLayer;
    private GameObject player;
    private bool isStunned = false;
    private float currentSpeed;

    [Header("Movement")]
    [SerializeField] private float moveForce = 20f;
    [SerializeField] private float maxSpeed = 5f;
    private Vector2 moveDirection;

    protected override void Start()
    {
        base.Start();

        // Setup rigidbody
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 0f;
        rb.linearDamping = 2f;
        rb.angularDamping = 2f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Find player
        player = GameObject.FindGameObjectWithTag("Player");

        // Initialize variables
        currentSpeed = speed;
        maxHealth = Mathf.Infinity;
        currentHealth = maxHealth;
    }

    protected override void Move()
    {
        if (isStunned || player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        // Only chase if within range
        if (distanceToPlayer <= chaseRange)
        {
            // Get direction to player
            Vector2 directionToPlayer = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;

            // Check for obstacles
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, obstacleAvoidanceRange, obstacleLayer);

            if (hit.collider != null)
            {
                // If there's an obstacle, try to move around it
                Vector2 avoidanceDirection = Vector2.zero;

                // Cast rays at different angles to find a clear path
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * 45f;
                    Vector2 direction = Quaternion.Euler(0, 0, angle) * directionToPlayer;
                    RaycastHit2D obstacleCheck = Physics2D.Raycast(transform.position, direction, obstacleAvoidanceRange, obstacleLayer);

                    if (obstacleCheck.collider == null)
                    {
                        avoidanceDirection = direction;
                        break;
                    }
                }

                // If we found a clear direction, use it
                if (avoidanceDirection != Vector2.zero)
                {
                    moveDirection = avoidanceDirection;
                }
                else
                {
                    // If no clear path, move along the wall
                    moveDirection = Vector2.Perpendicular(hit.normal);
                }
            }
            else
            {
                // No obstacles, move directly towards player
                moveDirection = directionToPlayer;
            }

            // Apply movement force
            rb.AddForce(moveDirection * moveForce);

            // Clamp velocity to max speed
            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);
        }
        else
        {
            // Slow down when out of range
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 2f);
        }

        // Debug visualization
        Debug.DrawRay(transform.position, moveDirection * 2f, Color.red);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerCapyScript player = collision.gameObject.GetComponent<PlayerCapyScript>();
            if (player != null)
            {
                player.Die();
            }
        }
        else if (collision.gameObject.CompareTag("Bomb"))
        {
            Stun();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Add bounce effect when colliding with obstacles
        if (((1 << collision.gameObject.layer) & obstacleLayer) != 0)
        {
            Vector2 bounceDirection = ((Vector2)transform.position - collision.contacts[0].point).normalized;
            rb.AddForce(bounceDirection * moveForce * 0.5f);
        }
    }

    public void Stun()
    {
        if (!isStunned)
        {
            StopAllCoroutines();
            StartCoroutine(ApplyStun());
        }
    }

    private IEnumerator ApplyStun()
    {
        isStunned = true;
        currentSpeed = 0f;
        rb.linearVelocity = Vector2.zero;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.blue;
        }

        yield return new WaitForSeconds(stunDuration);

        isStunned = false;
        currentSpeed = speed;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    public override void TakeDamage(float damage)
    {
        Debug.Log($"{gameObject.name} is unkillable. Ignoring damage.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, obstacleAvoidanceRange);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
