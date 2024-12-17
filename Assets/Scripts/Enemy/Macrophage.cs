using UnityEngine;
using System.Collections;

public class Macrophage : BaseEnemy
{
    [Header("Chase Settings")]
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] private float stunDuration = 2f;
    [SerializeField] private float obstacleAvoidanceRange = 1f;
    [SerializeField] private LayerMask obstacleLayer; // Set this in inspector to include walls/obstacles
    private GameObject player;
    private bool isStunned = false;
    private float currentSpeed;

    [Header("Obstacle Avoidance")]
    [SerializeField] private float raycastDistance = 1.5f;
    [SerializeField] private float avoidanceWeight = 1.5f;
    private Vector2 currentDirection;
    private Collider2D enemyCollider;

    protected override void Start()
    {
        base.Start();
        rb.bodyType = RigidbodyType2D.Dynamic; // Changed to Dynamic
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 0f; // Ensure no gravity affects the enemy
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent rotation
        rb.linearDamping = 5f; // Add some drag to prevent sliding

        player = GameObject.FindGameObjectWithTag("Player");
        maxHealth = Mathf.Infinity;
        currentHealth = maxHealth;
        currentSpeed = speed;
        currentDirection = Vector2.zero;

        enemyCollider = GetComponent<Collider2D>();
        if (enemyCollider != null)
        {
            enemyCollider.isTrigger = false; // Ensure the collider is not a trigger
        }
    }

    protected override void Move()
    {
        if (isStunned || player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= chaseRange)
        {
            Vector2 directionToPlayer = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
            Vector2 avoidanceDirection = HandleObstacleAvoidance();

            // Combine pursuit and avoidance
            Vector2 targetDirection = (directionToPlayer + avoidanceDirection * avoidanceWeight).normalized;

            // Smoothly interpolate the current direction
            currentDirection = Vector2.Lerp(currentDirection, targetDirection, Time.fixedDeltaTime * 5f);

            // Use AddForce instead of directly setting velocity
            rb.AddForce(currentDirection * currentSpeed * 10f);

            // Clamp velocity to prevent excessive speed
            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, currentSpeed);
        }
        else
        {
            // Gradually slow down when out of range
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 5f);
        }
    }

    private Vector2 HandleObstacleAvoidance()
    {
        Vector2 avoidanceDirection = Vector2.zero;
        float raySpread = 45f;
        int rayCount = 8;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = transform.eulerAngles.z - raySpread + (2 * raySpread * i / (rayCount - 1));
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, raycastDistance, obstacleLayer);
            Debug.DrawRay(transform.position, direction * raycastDistance, Color.yellow);

            if (hit.collider != null)
            {
                float weight = 1 - (hit.distance / raycastDistance);
                avoidanceDirection -= (Vector2)hit.normal * weight;
            }
        }

        return avoidanceDirection.normalized;
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
        // Add slight repulsion force when colliding with obstacles
        if (((1 << collision.gameObject.layer) & obstacleLayer) != 0)
        {
            Vector2 repulsionDirection = (transform.position - collision.transform.position).normalized;
            rb.AddForce(repulsionDirection * currentSpeed * 5f);
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
        Gizmos.DrawWireSphere(transform.position, raycastDistance);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
