using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class BrainMinion : BaseEnemy
{
    [Header("Health Settings")]
    [SerializeField] private new float maxHealth = 100f;

    [Header("Teleport Settings")]
    [SerializeField] private float teleportInterval = 5f;
    [SerializeField] private float teleportCooldown = 1f;
    private float teleportTimer;

    [Header("Lunge Attack Settings")]
    [SerializeField] private float lungeSpeed = 20f;
    [SerializeField] private float lungeDistance = 5f;
    [SerializeField] private float lungeWarningTime = 1f;
    [SerializeField] private float lungeCooldown = 3f;
    private bool isLunging = false;
    private bool isPreparingLunge = false;
    private Vector2 lungeDirection;

    [Header("Player Detection")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float latchRadius = 2f; // New param: if player is this close, latch onto player
    private GameObject player;

    [Header("Tilemap Constraints")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask enemyLayer;
    private List<Vector2> validTeleportPositions = new List<Vector2>();
    private float minionWidth;
    private float minionHeight;

    protected override void Start()
    {
        base.Start();
        currentHealth = maxHealth;

        // Minion is kinematic per requirements
        rb.bodyType = RigidbodyType2D.Kinematic;

        player = GameObject.FindGameObjectWithTag("Player");
        teleportTimer = teleportInterval;

        if (tilemap == null)
        {
            tilemap = FindObjectOfType<Tilemap>();
        }

        if (tilemap != null)
        {
            CalculateValidTeleportPositions();
        }
        else
        {
            Debug.LogWarning("Tilemap not assigned or found. Teleport may fail.");
        }

        // Calculate minion size for collision checks
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            minionWidth = spriteRenderer.bounds.size.x;
            minionHeight = spriteRenderer.bounds.size.y;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        // If player within latchRadius, latch onto player immediately
        if (!isLunging && !isPreparingLunge && distanceToPlayer <= latchRadius)
        {
            LatchOntoPlayer();
            return; // After latching, we can return
        }

        // Handle teleportation
        teleportTimer -= Time.deltaTime;
        if (teleportTimer <= 0f && !isLunging && !isPreparingLunge)
        {
            TeleportToRandomPosition();
            teleportTimer = teleportInterval + teleportCooldown;
        }

        // Check for lunge conditions (only if not latched)
        if (!isLunging && !isPreparingLunge && distanceToPlayer <= detectionRange && distanceToPlayer > latchRadius)
        {
            StartCoroutine(PrepareLunge());
        }
    }

    private void CalculateValidTeleportPositions()
    {
        validTeleportPositions.Clear();
        if (tilemap == null) return;

        // Use tilemap bounds
        BoundsInt bounds = tilemap.cellBounds;
        Vector3 tileSize = tilemap.cellSize;

        // Iterate over each cell in tilemap area
        for (int x = bounds.xMin; x <= bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y <= bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);

                // Check if this cell has a tile (meaning ground)
                if (tilemap.HasTile(cellPosition))
                {
                    Vector3 cellCenter = tilemap.CellToWorld(cellPosition) + (tileSize / 2f);

                    // Ensure no obstacles or enemies at the position and minion fits here
                    if (!IsPositionOccupied(cellCenter))
                    {
                        validTeleportPositions.Add(cellCenter);
                    }
                }
            }
        }

        //Debug.Log($"Valid teleport positions calculated: {validTeleportPositions.Count} positions available.");
    }

    private bool IsPositionOccupied(Vector2 position)
    {
        // We check if placing the minion here overlaps obstacles/enemies
        // OverlapBox: size = minionWidth x minionHeight
        Collider2D hit = Physics2D.OverlapBox(position, new Vector2(minionWidth, minionHeight), 0f, obstacleLayer | enemyLayer);
        return hit != null;
    }

    private void TeleportToRandomPosition()
    {
        if (validTeleportPositions.Count == 0)
        {
            Debug.LogError("No valid teleport positions available.");
            return;
        }

        int randomIndex = Random.Range(0, validTeleportPositions.Count);
        Vector2 teleportPosition = validTeleportPositions[randomIndex];
        transform.position = teleportPosition;

        //Debug.Log($"Brain Minion teleported to {teleportPosition}");
    }

    private IEnumerator PrepareLunge()
    {
        if (isLunging || isPreparingLunge) yield break;
        isPreparingLunge = true;

        float flashInterval = 0.2f;
        float flashTimer = 0f;

        // Flash red/white as warning
        while (flashTimer < lungeWarningTime)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.red;
                yield return new WaitForSeconds(flashInterval);
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(flashInterval);
            }
            flashTimer += flashInterval * 2;
        }

        StartCoroutine(Lunge());
        isPreparingLunge = false;
    }

    private IEnumerator Lunge()
    {
        if (player == null) yield break;

        isLunging = true;

        Vector2 targetPosition = player.transform.position;
        Vector2 startPosition = transform.position;
        lungeDirection = (targetPosition - startPosition).normalized;

        float distanceCovered = 0f;

        while (distanceCovered < lungeDistance)
        {
            float moveStep = lungeSpeed * Time.deltaTime;
            distanceCovered += moveStep;

            Vector2 newPosition = (Vector2)transform.position + lungeDirection * moveStep;

            // Check if hitting a wall (tile present at that cell)
            Vector3Int cellPosition = tilemap.WorldToCell(newPosition);
            if (tilemap != null && tilemap.HasTile(cellPosition) == false)
            {
                // If no tile at target cell, might be off-walkable area, stop lunge
                //Debug.Log("Lunge interrupted by non-walkable area (no tile).");
                break;
            }

            transform.position = newPosition;
            yield return null;
        }

        isLunging = false;
        yield return new WaitForSeconds(lungeCooldown);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //Debug.Log("Brain Minion collided with Player � Player is Dead!");
            PlayerCapyScript player = collision.gameObject.GetComponent<PlayerCapyScript>();
            if (player != null)
            {
                player.Die(); // Trigger player death
            }
        }
    }
    
    // Latch onto player if within latch radius
    private void LatchOntoPlayer()
    {
        if (player == null) return;
        //Debug.Log("Brain Minion latches onto the player!");
        // Teleport directly onto the player
        transform.position = player.transform.position;

        // Potentially deal damage, apply effects, etc.
        // This is a direct latch action. Adjust as needed.
    }

    public override void TakeDamage(float damage)
    {
        currentHealth -= damage;
        base.TakeDamage(damage);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public override void Die()
    {
        //Debug.Log("Brain Minion has been defeated!");
        base.Die();
    }

    private void OnDrawGizmosSelected()
    {
        if (tilemap != null && validTeleportPositions != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Vector2 position in validTeleportPositions)
            {
                Gizmos.DrawSphere(position, 0.1f);
            }
        }
    }
}
