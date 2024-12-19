using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    [SerializeField] private int testPhase = 0; // Test each phase
    private int currentPhase = 1;

    [Header("Phase 1: Bullet Waves")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private int bulletsPerWave = 15;
    [SerializeField] private float bulletFireRate = 0.5f;

    [Header("Phase 2: Summon Minions")]
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private int minionsToSpawn = 5;
    [SerializeField] private float minionSpawnDistance = 1f;
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

        Vector2 directionToPlayer = ((Vector2)player.transform.position - (Vector2)bulletSpawnPoint.position).normalized;
        float baseAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

        float angleStep = 180f / (bulletsPerWave - 1);
        float startAngle = baseAngle - 90f;

        for (int i = 0; i < bulletsPerWave; i++)
        {
            float angle = startAngle + (i * angleStep);
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
            bullet.GetComponent<Rigidbody2D>().linearVelocity = direction * 5f;
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
        yield return StartCoroutine(Phase2_SpawnAndLaunchMinions());
        Instantiate(laserPrefab, laserSpawnPoint.position, Quaternion.identity);
        yield return new WaitForSeconds(laserFireRate);
    }

    private IEnumerator RushAttack()
    {
        if (player == null) yield break;
        isRushing = true;

        while (Vector2.Distance(transform.position, player.transform.position) > stopDistance)
        {
            Vector2 newPosition = Vector2.MoveTowards(transform.position, player.transform.position, rushSpeed * Time.deltaTime);
            transform.position = newPosition;
            yield return null;
        }

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Laser"))
        {
            TakeDamage(10);
        }
        else if (collision.gameObject.CompareTag("Bomb"))
        {
            TakeDamage(20);
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            PlayerCapyScript player = collision.gameObject.GetComponent<PlayerCapyScript>();
            if (player != null)
            {
                player.Die();
            }
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minionSpawnDistance);
    }
}
