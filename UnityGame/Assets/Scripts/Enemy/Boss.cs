using UnityEngine;
using System.Collections;

public class Boss : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float bossMaxHealth = 1500f;
    [SerializeField] private float shieldMaxHealth = 500f;
    [SerializeField] private float shieldRegenCooldown = 20f;

    private float currentBossHealth;
    private float currentShieldHealth;
    private bool shieldActive = true;

    [Header("Phases & Timers")]
    [SerializeField] private float phaseSwitchInterval = 20f;
    [SerializeField] private float phaseDuration = 15f;
    [SerializeField] private int testPhase = 0;
    private int currentPhase = 1;

    [Header("Phase 1: Bullet Waves")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpawnDistance = 1.5f;
    [SerializeField] private int bulletsPerWave = 15;
    [SerializeField] private float bulletFireRate = 0.5f;
    [SerializeField] private float bulletSpeed = 5f;

    [Header("Phase 2: Summon Minions")]
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private int minionsToSpawn = 5;
    [SerializeField] private float minionSpawnDistance = 1.5f;
    [SerializeField] private float minionLaunchForce = 5f;

    [Header("Phase 3: Random Lasers")]
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private Transform laserSpawnPoint;
    [SerializeField] private float laserFireRate = 1f;

    [Header("Rush Attack")]
    [SerializeField] private float rushInterval = 30f;
    [SerializeField] private float rushSpeed = 15f;
    [SerializeField] private float stopDistance = 2f;
    private bool isRushing = false;

    [Header("Shield Visuals")]
    [SerializeField] private GameObject shieldVisual;

    private GameObject player;
    private Rigidbody2D rb;
    private float nextRushTime;

    private void Start()
    {
        currentBossHealth = bossMaxHealth;
        currentShieldHealth = shieldMaxHealth;
        shieldActive = true;

        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        player = GameObject.FindGameObjectWithTag("Player");
        nextRushTime = Time.time + rushInterval;

        if (testPhase == 0)
        {
            StartCoroutine(PhaseCycle());
        }
        else
        {
            StartCoroutine(TriggerPhase(testPhase));
        }
    }

    private void Update()
    {
        if (currentBossHealth <= 0)
        {
            Die();
        }

        rb.linearVelocity = Vector2.zero;

        if (Time.time >= nextRushTime && !isRushing)
        {
            StartCoroutine(RushAttack());
            nextRushTime = Time.time + rushInterval;
        }
    }

    private IEnumerator PhaseCycle()
    {
        while (currentBossHealth > 0)
        {
            yield return StartCoroutine(TriggerPhase(currentPhase));
            currentPhase = currentPhase >= 3 ? 1 : currentPhase + 1;
            yield return new WaitForSeconds(phaseSwitchInterval);
        }
    }

    private IEnumerator TriggerPhase(int phase)
    {
        switch (phase)
        {
            case 1:
                yield return StartCoroutine(Phase1_BulletWaves());
                break;
            case 2:
                yield return StartCoroutine(Phase2_SpawnAndLaunchMinions());
                break;
            case 3:
                yield return StartCoroutine(Phase3_AllAttacks());
                break;
        }
    }

    private IEnumerator Phase1_BulletWaves()
    {
        float phaseEndTime = Time.time + phaseDuration;
        while (Time.time < phaseEndTime)
        {
            FireSemiCircleBullets();
            yield return new WaitForSeconds(bulletFireRate);
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

            // Rotate the bullet to face 'angle'
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            GameObject bullet = Instantiate(bulletPrefab, spawnPosition, rotation);

            // No need to set velocity here since bullet handles its own movement via Translate.
            // Just ensure that transform.right is pointing in the correct firing direction.
        }
    }


    private IEnumerator Phase2_SpawnAndLaunchMinions()
    {
        for (int i = 0; i < minionsToSpawn; i++)
        {
            SpawnAndLaunchMinion();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void SpawnAndLaunchMinion()
    {
        Vector2 spawnPosition = (Vector2)transform.position + Random.insideUnitCircle.normalized * minionSpawnDistance;
        Vector2 launchDirection = (spawnPosition - (Vector2)transform.position).normalized;

        GameObject minion = Instantiate(minionPrefab, spawnPosition, Quaternion.identity);
        Rigidbody2D minionRb = minion.GetComponent<Rigidbody2D>();

        if (minionRb != null)
        {
            minionRb.linearVelocity = launchDirection * Random.Range(minionLaunchForce * 0.8f, minionLaunchForce * 1.2f);
        }
    }

    private IEnumerator Phase3_AllAttacks()
    {
        // Spawn and launch minions first (as original code)
        //yield return StartCoroutine(Phase2_SpawnAndLaunchMinions());

        // Now fire multiple lasers at intervals
        int numberOfLasers = 5; // for example, fire 5 lasers
        for (int i = 0; i < numberOfLasers; i++)
        {
            FireLaserAtPlayer();
            yield return new WaitForSeconds(laserFireRate);
        }
    }

    private void FireLaserAtPlayer()
    {
        if (player == null) return;

        // Calculate direction to player
        Vector2 directionToPlayer = (player.transform.position - laserSpawnPoint.position).normalized;
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

        // Rotate the laser to face the player
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        Instantiate(laserPrefab, laserSpawnPoint.position, rotation);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision is with the player
        if (collision.gameObject.CompareTag("Player"))
        {
            //Debug.Log("Player hit");
            PlayerCapyScript player = collision.gameObject.GetComponent<PlayerCapyScript>();
            if (player != null)
            {
                player.Die(); // Kill the player
            }
        }
        // Check if the collision is with an obstacle
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            //Debug.Log("Collided with an obstacle");

            // Stop the boss's movement
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero; // Stop any ongoing movement
                rb.angularVelocity = 0f;   // Stop any rotational movement
            }

            // Adjust the position slightly away from the obstacle
            Vector2 collisionNormal = collision.GetContact(0).normal;
            transform.position += (Vector3)collisionNormal * 0.1f;
        }
    }

    private IEnumerator RushAttack()
    {
        if (player == null) yield break;
        isRushing = true;

        float rushDuration = 2f; // Set how long the boss will chase the player
        float rushStartTime = Time.time;
        float obstacleAvoidanceDistance = 1f; // Distance to check for obstacles

        while (Time.time < rushStartTime + rushDuration)
        {
            if (player == null) break;

            // Calculate direction to player
            Vector2 directionToPlayer = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;

            // Use raycast to check for obstacles
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, obstacleAvoidanceDistance, LayerMask.GetMask("Obstacle"));

            if (hit.collider != null)
            {
                // If there's an obstacle, find a way around it
                Vector2 avoidanceDirection = Vector2.Perpendicular(hit.normal).normalized;

                // Determine which side to move (left or right around the obstacle)
                Vector2 testPosition1 = (Vector2)transform.position + avoidanceDirection * obstacleAvoidanceDistance;
                Vector2 testPosition2 = (Vector2)transform.position - avoidanceDirection * obstacleAvoidanceDistance;

                // Use the side with the least obstruction
                if (Physics2D.Raycast(testPosition1, directionToPlayer, obstacleAvoidanceDistance, LayerMask.GetMask("Obstacle")).collider == null)
                {
                    directionToPlayer = (testPosition1 - (Vector2)transform.position).normalized;
                }
                else if (Physics2D.Raycast(testPosition2, directionToPlayer, obstacleAvoidanceDistance, LayerMask.GetMask("Obstacle")).collider == null)
                {
                    directionToPlayer = (testPosition2 - (Vector2)transform.position).normalized;
                }
            }

            // Move towards the player or around the obstacle
            Vector2 newPosition = Vector2.MoveTowards(transform.position, (Vector2)transform.position + directionToPlayer, rushSpeed * Time.deltaTime);
            transform.position = newPosition;

            yield return null; // Wait for the next frame
        }

        // Stop rushing and wait for a cooldown
        yield return new WaitForSeconds(2f);
        isRushing = false;
    }


    public void TakeDamage(float damage)
    {
        if (shieldActive)
        {
            currentShieldHealth -= damage;
            if (currentShieldHealth <= 0)
            {
                shieldActive = false;
                shieldVisual.SetActive(false);
                StartCoroutine(RegenerateShield());
            }
        }
        else
        {
            currentBossHealth -= damage;
        }
    }

    private IEnumerator RegenerateShield()
    {
        yield return new WaitForSeconds(shieldRegenCooldown);
        currentShieldHealth = shieldMaxHealth;
        shieldActive = true;
        shieldVisual.SetActive(true);
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, bulletSpawnDistance);
    }
}
