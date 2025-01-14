using UnityEngine;
using System.Collections;

public class Neutrophil : BaseEnemy
{
    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 2f;
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] private float obstacleAvoidanceRange = 1f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Movement")]
    [SerializeField] private float moveForce = 20f;
    [SerializeField] private float maxSpeed = 5f;
    private Vector2 moveDirection;

    [Header("Explosion Settings")]
    [SerializeField] private float explosionDelay = 2f;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float explosionDamage = 9999f;
    [SerializeField] private GameObject explosionEffect;

    [Header("Flash Settings")]
    [SerializeField] private float flashInterval = 0.2f;
    [SerializeField] private Color flashColor = Color.red;

    private bool isExploding = false;
    private GameObject player;
    private Animator animator; // Reference to the Animator

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

        player = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponent<Animator>(); // Get the Animator component
    }

    protected override void Move()
    {
        if (isExploding || player == null)
        {
            rb.linearVelocity = Vector2.zero;
            UpdateAnimator(false); // Not moving
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
            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, chaseSpeed);

            // Update Animator based on movement
            UpdateAnimator(rb.linearVelocity.magnitude > 0.1f);
        }
        else
        {
            // Slow down when out of range
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 2f);
            UpdateAnimator(rb.linearVelocity.magnitude > 0.1f); // Update Animator
        }
    }

    private void UpdateAnimator(bool isMoving)
    {
        if (animator != null)
        {
            animator.SetBool("Moving", isMoving);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isExploding) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            //Debug.Log("Neutrophil collided with Player — Exploding!");
            Explode();
        }
        else if (collision.gameObject.CompareTag("Laser") || collision.gameObject.CompareTag("Bomb"))
        {
            //Debug.Log("Neutrophil hit by Capy Lazer — Detonating remotely!");
            Explode();
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

    public void Explode()
    {
        if (isExploding) return;
        isExploding = true;

        rb.linearVelocity = Vector2.zero;
        UpdateAnimator(false); // Stop movement during explosion
        StartCoroutine(FlashBeforeExplosion());
        Invoke(nameof(TriggerExplosion), explosionDelay);
    }

    private IEnumerator FlashBeforeExplosion()
    {
        if (spriteRenderer == null) yield break;

        float elapsedTime = 0f;

        while (elapsedTime < explosionDelay)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashInterval);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashInterval);
            elapsedTime += flashInterval * 2;
        }
    }

    private void TriggerExplosion()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D obj in hitObjects)
        {
            if (obj.CompareTag("Player"))
            {
                PlayerCapyScript player = obj.GetComponent<PlayerCapyScript>();
                if (player != null)
                {
                    //Debug.Log("Player hit by Neutrophil Explosion — Instantly Dead!");
                    player.Die();
                }
            }
        }

        Die();
    }

    private void OnDrawGizmosSelected()
    {
        // Draw explosion radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        // Draw chase range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // Draw obstacle avoidance range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, obstacleAvoidanceRange);
    }
}
