using UnityEngine;
using System.Collections;

public class Macrophage : BaseEnemy
{
    [Header("Chase Settings")]
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] private float stunDuration = 2f;

    // The layer(s) to treat as obstacles (walls, etc.).
    // Be sure NOT to include the Player layer here.
    [SerializeField] private LayerMask obstacleLayer;

    [Tooltip("Number of directions (around a circle) to check for a free path if blocked.")]
    [SerializeField] private int avoidanceSamples = 16;

    // If we can't find a free direction after all samples, we revert to direct line movement.
    [SerializeField] private float obstacleAvoidanceRange = 1f;

    private GameObject player;
    private bool isStunned = false;

    [Header("Movement")]
    [SerializeField] private float moveForce = 20f;
    [SerializeField] private float maxSpeed = 5f;
    private Vector2 moveDirection;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Flicker settings
    [SerializeField] private float flickerDuration = 0.2f;
    [SerializeField] private int flickerCount = 2;
    private Color originalColor;

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

        // Initialize health
        maxHealth = 100f;
        currentHealth = maxHealth;

        // Setup references
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    protected override void Move()
    {
        if (isStunned || player == null)
        {
            rb.linearVelocity = Vector2.zero;
            SetAnimatorMoving(false);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= chaseRange)
        {
            Vector2 directionToPlayer = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;

            // Check for obstacles EXCEPT the player
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                directionToPlayer,
                obstacleAvoidanceRange,
                obstacleLayer // Do NOT include the player layer here
            );

            if (hit.collider != null)
            {
                // Attempt to find a free direction
                Vector2 avoidanceDirection = FindAvoidanceDirection(directionToPlayer);

                // If we found a free direction, use it; otherwise just continue toward the player
                if (avoidanceDirection != Vector2.zero)
                {
                    moveDirection = avoidanceDirection;
                }
                else
                {
                    // Worst case: move directly at the player 
                    // (ensures we don't get stuck if there's no immediately free direction)
                    moveDirection = directionToPlayer;
                }
            }
            else
            {
                // No obstacle, go straight to player
                moveDirection = directionToPlayer;
            }

            rb.AddForce(moveDirection * moveForce);
            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);
            SetAnimatorMoving(true);
        }
        else
        {
            // Player out of range, slow to a stop
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 2f);
            SetAnimatorMoving(false);
        }

        Debug.DrawRay(transform.position, moveDirection * 2f, Color.red);
    }

    /// <summary>
    /// Tries multiple angles around 'baseDirection' to find a direction not blocked by an obstacle.
    /// Returns Vector2.zero if none found.
    /// </summary>
    private Vector2 FindAvoidanceDirection(Vector2 baseDirection)
    {
        // We'll sample in small angle increments around the baseDirection
        float angleIncrement = 360f / avoidanceSamples;

        // If we find a direction with no obstacle, we return it immediately
        for (int i = 0; i < avoidanceSamples; i++)
        {
            float angle = i * angleIncrement;
            // Rotate baseDirection by 'angle' degrees
            Vector2 checkDir = Quaternion.Euler(0, 0, angle) * baseDirection;

            RaycastHit2D obstacleCheck = Physics2D.Raycast(
                transform.position,
                checkDir,
                obstacleAvoidanceRange,
                obstacleLayer
            );

            // If we didn't hit an obstacle, we can move in this direction
            if (obstacleCheck.collider == null)
            {
                return checkDir.normalized;
            }
        }

        // No free direction found
        return Vector2.zero;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1) If we collide with Player, kill the player
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerCapyScript playerScript = collision.gameObject.GetComponent<PlayerCapyScript>();
            if (playerScript != null)
            {
                playerScript.Die(); // Kill player on contact
                Debug.Log("Player killed on contact by Macrophage.");
            }
        }
        // 2) If we collide with a Bomb, stun this Macrophage
        else if (collision.gameObject.CompareTag("Bomb"))
        {
            Stun();
        }
        // 3) For obstacles or other enemies, do a small bounce
        else
        {
            // If it's an obstacle or another Macrophage/enemy, apply a slight bounce
            int collisionLayer = collision.gameObject.layer;
            if (((1 << collisionLayer) & obstacleLayer) != 0 || collision.gameObject.GetComponent<BaseEnemy>() != null)
            {
                // Take the first contact normal
                Vector2 bounceDirection = collision.contacts[0].normal;
                rb.AddForce(bounceDirection * 5f, ForceMode2D.Impulse); // Adjust bounce force as needed
            }
        }
    }

    public override void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"{name} took {damage} damage. Remaining health: {currentHealth}");

        // Flicker on damage
        if (spriteRenderer != null)
        {
            StartCoroutine(DamageFlickerCoroutine());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public override void Die()
    {
        Debug.Log($"{name} has been killed!");
        // Add any death effects or animations here
        Destroy(gameObject);
    }

    public void Stun()
    {
        if (!isStunned)
        {
            StopAllCoroutines(); // Stop any ongoing flickers, etc.
            StartCoroutine(ApplyStun());
        }
    }

    private IEnumerator ApplyStun()
    {
        isStunned = true;
        rb.linearVelocity = Vector2.zero;
        SetAnimatorMoving(false);

        // Change color or play stun animation
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.blue;
        }

        yield return new WaitForSeconds(stunDuration);

        isStunned = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    /// <summary>
    /// Flicker the sprite a few times when damaged.
    /// </summary>
    private IEnumerator DamageFlickerCoroutine()
    {
        float flickerInterval = flickerDuration / (flickerCount * 2f);
        for (int i = 0; i < flickerCount; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(flickerInterval);

            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(flickerInterval);
        }
    }

    private void SetAnimatorMoving(bool isMoving)
    {
        if (animator != null)
        {
            animator.SetBool("Moving", isMoving);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
