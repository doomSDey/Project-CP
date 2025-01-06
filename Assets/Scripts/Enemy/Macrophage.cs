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

        // Initialize health
        maxHealth = 100f; // Set to desired health
        currentHealth = maxHealth;
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

            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, obstacleAvoidanceRange, obstacleLayer);
            if (hit.collider != null)
            {
                Vector2 avoidanceDirection = Vector2.zero;
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

                moveDirection = avoidanceDirection != Vector2.zero ? avoidanceDirection : Vector2.Perpendicular(hit.normal);
            }
            else
            {
                moveDirection = directionToPlayer;
            }

            rb.AddForce(moveDirection * moveForce);
            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);
        }
        else
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 2f);
        }

        Debug.DrawRay(transform.position, moveDirection * 2f, Color.red);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerCapyScript playerScript = collision.gameObject.GetComponent<PlayerCapyScript>();
            if (playerScript != null)
            {
                playerScript.Die(); // Kill player on contact
                Debug.Log("Player killed on contact.");
            }
        }
        else if (collision.gameObject.CompareTag("Bomb"))
        {
            Stun();
        }
    }

    public override void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Remaining health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public override void Die()
    {
        Debug.Log($"{gameObject.name} has been killed!");
        // Add any death effects or animations here
        Destroy(gameObject);
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
        rb.linearVelocity = Vector2.zero;

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
