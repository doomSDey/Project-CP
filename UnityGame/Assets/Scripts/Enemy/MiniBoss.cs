using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MiniBoss : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float miniBossMaxHealth = 5000f;
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

    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer; // For flicker effect
    [SerializeField] private Color damageColor = Color.red; // Color to flicker when damaged
    [SerializeField] private float flickerDuration = 0.2f; // Duration of one flicker cycle
    [SerializeField] private int flickerCount = 3; // Number of flickers when damaged

    private GameObject player;
    private Rigidbody2D rb;
    private Animator animator; // Animator reference
    private int score; // Player's score
    private bool facingRight = true; // To track the current facing direction
    private Color originalColor; // Store the original color for flicker reset

    private void Start()
    {
        currentMiniBossHealth = miniBossMaxHealth;

        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        player = GameObject.FindGameObjectWithTag("Player");

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("Animator not found! Please attach an Animator component.");
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer not found! Please attach a SpriteRenderer component.");
        }
        else
        {
            originalColor = spriteRenderer.color; // Store the original color
        }

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
            FireSemiCircleBullets();
            StartCoroutine(RushTowardsPlayer());
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

            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, stopDistance, obstacleLayer);
            if (hit.collider != null)
            {
                Vector2 collisionNormal = hit.normal;
                direction = Vector2.Perpendicular(collisionNormal).normalized;
            }

            FlipSprite(direction.x);

            Vector2 newPosition = Vector2.MoveTowards(transform.position, (Vector2)transform.position + direction, rushSpeed * Time.deltaTime);
            transform.position = newPosition;

            UpdateAnimator(true);

            yield return null;
        }

        UpdateAnimator(false);

        yield return new WaitForSeconds(1f);
        isRushing = false;
    }

    private void UpdateAnimator(bool isMoving)
    {
        if (animator != null)
        {
            animator.SetBool("Moving", isMoving);
        }
    }

    private void FlipSprite(float directionX)
    {
        if ((directionX > 0 && !facingRight) || (directionX < 0 && facingRight))
        {
            facingRight = !facingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    public void TakeDamage(float damage)
    {
        currentMiniBossHealth -= damage;
        //Debug.Log($"MiniBoss took {damage} damage. Remaining health: {currentMiniBossHealth}");
        StartCoroutine(FlickerEffect());
    }

    private IEnumerator FlickerEffect()
    {
        if (spriteRenderer == null) yield break;

        for (int i = 0; i < flickerCount; i++)
        {
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(flickerDuration / (flickerCount * 2f));
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flickerDuration / (flickerCount * 2f));
        }
    }

    private void Die()
    {
        //Debug.Log("MiniBoss defeated!");
        UpdateAnimator(false);
        AwardPoints(1000);
        SceneManager.LoadScene("GameFin");
        Destroy(gameObject);
    }

    private void AwardPoints(int points)
    {
        score += points;
        //Debug.Log($"Player awarded {points} points! Total score: {score}");
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
            Bomb bomb = collision.gameObject.GetComponent<Bomb>();
            if (bomb != null)
            {
                TakeDamage(bomb.damage);
            }
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Laser"))
        {
            Laser laser = collision.gameObject.GetComponent<Laser>();
            if (laser != null)
            {
                TakeDamage(laser.damage);
            }
            Destroy(collision.gameObject);
        }
    }
}
