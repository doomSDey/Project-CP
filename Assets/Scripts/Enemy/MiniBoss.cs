using UnityEngine;
using System.Collections;

public class MiniBoss : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float miniBossMaxHealth = 500f;

    private float currentMiniBossHealth;

    [Header("Bullet Waves")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpawnDistance = 1.5f;
    [SerializeField] private int bulletsPerWave = 10;
    [SerializeField] private float bulletFireRate = 1f;
    [SerializeField] private float bulletSpeed = 4f;

    [Header("Rush Attack")]
    [SerializeField] private float rushSpeed = 10f;
    [SerializeField] private float stopDistance = 2f;
    [SerializeField] private float rushInterval = 5f;
    private bool isRushing = false;

    [Header("Collision Handling")]
    [SerializeField] private LayerMask obstacleLayer;

    private GameObject player;
    private Rigidbody2D rb;

    private void Start()
    {
        currentMiniBossHealth = miniBossMaxHealth;

        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        player = GameObject.FindGameObjectWithTag("Player");

        StartCoroutine(AttackCycle());
    }

    private void Update()
    {
        if (currentMiniBossHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator AttackCycle()
    {
        while (currentMiniBossHealth > 0)
        {
            // Fire a wave of bullets
            FireSemiCircleBullets();

            // Rush in the direction of the player
            StartCoroutine(RushTowardsPlayer());

            // Wait for the next attack cycle
            yield return new WaitForSeconds(rushInterval);
        }
    }

    private void FireSemiCircleBullets()
    {
        if (player == null) return;

        Vector2 directionToPlayer = (player.transform.position - transform.position).normalized;
        float baseAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

        float angleStep = 180f / (bulletsPerWave - 1);
        float startAngle = baseAngle - 90f;

        for (int i = 0; i < bulletsPerWave; i++)
        {
            float angle = startAngle + (i * angleStep);
            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            Vector2 spawnPosition = (Vector2)transform.position + direction * bulletSpawnDistance;

            GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.Euler(0, 0, angle));
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.linearVelocity = direction * bulletSpeed;
            }
        }
    }

    private IEnumerator RushTowardsPlayer()
    {
        if (player == null || isRushing) yield break;
        isRushing = true;

        Vector2 targetPosition = player.transform.position;

        while (Vector2.Distance(transform.position, targetPosition) > stopDistance)
        {
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;

            // Use raycast to check for obstacles
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, stopDistance, obstacleLayer);
            if (hit.collider != null)
            {
                // If an obstacle is detected, stop or adjust direction
                Vector2 collisionNormal = hit.normal;
                direction = Vector2.Perpendicular(collisionNormal).normalized;
            }

            Vector2 newPosition = Vector2.MoveTowards(transform.position, (Vector2)transform.position + direction, rushSpeed * Time.deltaTime);
            transform.position = newPosition;

            yield return null;
        }

        yield return new WaitForSeconds(1f); // Pause before the next attack
        isRushing = false;
    }

    public void TakeDamage(float damage)
    {
        currentMiniBossHealth -= damage;
        Debug.Log($"MiniBoss took {damage} damage. Remaining health: {currentMiniBossHealth}");
    }

    private void Die()
    {
        Debug.Log("MiniBoss defeated!");
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision is with the player
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player hit");
            PlayerCapyScript player = collision.gameObject.GetComponent<PlayerCapyScript>();
            if (player != null)
            {
                player.Die(); // Kill the player
            }
        }
        // Check if the collision is with an obstacle
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Collided with an obstacle");

            // Stop the miniboss's movement
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero; // Stop any ongoing movement
            }

            // Adjust position slightly away from the obstacle
            Vector2 collisionNormal = collision.GetContact(0).normal;
            transform.position += (Vector3)collisionNormal * 0.1f;
        }
    }
}
