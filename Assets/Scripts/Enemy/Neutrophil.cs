using UnityEngine;
using System.Collections;

public class Neutrophil : BaseEnemy
{
    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 2f; // Speed of movement
    [SerializeField] private float chaseRange = 10f; // Detection range to chase player

    [Header("Explosion Settings")]
    [SerializeField] private float explosionDelay = 2f; // Time before explosion
    [SerializeField] private float explosionRadius = 2f; // AoE explosion radius
    [SerializeField] private float explosionDamage = 9999f; // Damage from explosion (guaranteed kill)
    [SerializeField] private GameObject explosionEffect; // Particle effect prefab for explosion

    [Header("Flash Settings")]
    [SerializeField] private float flashInterval = 0.2f; // How fast it flashes before explosion
    [SerializeField] private Color flashColor = Color.red; // Color to flash before explosion

    private bool isExploding = false; // Is it currently in the process of exploding?
    private GameObject player; // Reference to the player

    protected override void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player"); // Find the player in the scene
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void Move()
    {
        if (isExploding) return; // Don't move if it's in the process of exploding

        if (player == null) return;

        // Check if player is within chase range
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= chaseRange)
        {
            // Move towards the player
            Vector2 targetPosition = player.transform.position;
            Vector2 newPosition = Vector2.MoveTowards(transform.position, targetPosition, chaseSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isExploding) return; // Prevent multiple explosions

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Neutrophil collided with Player � Exploding!");
            Explode();
        }
        else if (collision.gameObject.CompareTag("Lazer"))
        {
            Debug.Log("Neutrophil hit by Capy Lazer � Detonating remotely!");
            Explode();
        }
    }

    /// <summary>
    /// Triggers the self-destruct explosion of the Neutrophil.
    /// </summary>
    public void Explode()
    {
        if (isExploding) return; // If it's already exploding, exit
        isExploding = true;

        // Stop all movement
        rb.linearVelocity = Vector2.zero;

        // Flash red for warning before the explosion
        StartCoroutine(FlashBeforeExplosion());

        // After a short delay, trigger the explosion
        Invoke(nameof(TriggerExplosion), explosionDelay);
    }

    /// <summary>
    /// Flash the Neutrophil to warn the player before it explodes.
    /// </summary>
    private IEnumerator FlashBeforeExplosion()
    {
        if (spriteRenderer == null) yield break;

        float elapsedTime = 0f;

        while (elapsedTime < explosionDelay)
        {
            spriteRenderer.color = flashColor; // Set flash color
            yield return new WaitForSeconds(flashInterval);
            spriteRenderer.color = originalColor; // Revert back to original color
            yield return new WaitForSeconds(flashInterval);
            elapsedTime += flashInterval * 2;
        }
    }

    /// <summary>
    /// Executes the explosion, dealing area-of-effect (AoE) damage.
    /// </summary>
    private void TriggerExplosion()
    {
        // Display explosion effect
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Damage all objects within explosion radius
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D obj in hitObjects)
        {
            if (obj.CompareTag("Player"))
            {
                PlayerCapyScript player = obj.GetComponent<PlayerCapyScript>();
                if (player != null)
                {
                    Debug.Log("Player hit by Neutrophil Explosion � Instantly Dead!");
                    player.Die();
                }
            }
        }

        // Destroy the Neutrophil
        Die();
    }

    /// <summary>
    /// Draws the explosion radius as a wireframe circle in the Unity Editor.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
