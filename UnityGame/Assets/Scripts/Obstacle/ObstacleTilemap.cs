using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(CompositeCollider2D))]
public class ObstacleTilemap : MonoBehaviour
{
    [Header("Bounce Settings")]
    [SerializeField] private bool canBounceBack = true;
    [SerializeField] private float frameDuration = 1f; // Duration of each animation frame
    [SerializeField] private int totalFrames = 4; // Total number of frames in the animation
    [SerializeField] private float pushBackForce = 3f; // Force to push back objects
    private float animationTimer; // Tracks time elapsed in the animation cycle

    [Header("Health Settings")]
    [SerializeField] private bool isDestroyable = false;
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isFlickering = false;
    private bool isAlive = true; // New flag to track if the object is alive

    private CompositeCollider2D compositeCollider;

    private void Start()
    {
        compositeCollider = GetComponent<CompositeCollider2D>();
        if (compositeCollider == null)
        {
            Debug.LogError("CompositeCollider2D is required for this script to work.");
            return;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        currentHealth = maxHealth;
        animationTimer = 0f; // Start the animation cycle timer
    }

    private void Update()
    {
        if (!canBounceBack || compositeCollider == null) return;

        // Update the animation timer
        animationTimer += Time.deltaTime;

        // Calculate the total duration of the animation cycle
        float animationCycleDuration = frameDuration * totalFrames;

        // Check if the animation cycle has ended
        if (animationTimer >= animationCycleDuration)
        {
            // Trigger push back
            TriggerPushBack();

            // Reset the animation timer for the next cycle
            animationTimer -= animationCycleDuration;
        }
    }

    /// <summary>
    /// Handles the logic to push back the player.
    /// </summary>
    private void TriggerPushBack()
    {
        // Check for collisions with the Composite Collider
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Player")); // Ensure the Player is on the Player layer
        filter.useTriggers = false;

        Collider2D[] results = new Collider2D[10];
        int hitCount = compositeCollider.Overlap(filter, results);

        if (hitCount > 0)
        {
            foreach (var hitCollider in results)
            {
                if (hitCollider == null) continue;

                Rigidbody2D playerRb = hitCollider.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    PushBack(playerRb);
                }
            }
        }
    }

    /// <summary>
    /// Push the player back from the Tilemap.
    /// </summary>
    private void PushBack(Rigidbody2D playerRb)
    {
        if (playerRb == null) return;

        // Calculate push direction from player to the closest point on the CompositeCollider
        Vector2 closestPoint = compositeCollider.ClosestPoint(playerRb.position);
        Vector2 direction = (playerRb.position - closestPoint).normalized;

        // Apply the force directly via the player's ApplyPushBack method
        PlayerCapyScript playerScript = playerRb.GetComponent<PlayerCapyScript>();
        if (playerScript != null)
        {
            playerScript.ApplyPushBack(direction * pushBackForce);
        }

        Debug.Log($"Pushing player back in direction {direction} with force {pushBackForce}");
    }

    public void TakeDamage(int damage)
    {
        if (!isDestroyable) return;

        currentHealth -= damage;
        StartFlicker();

        if (currentHealth <= 0)
        {
            isAlive = false; // Object is no longer "alive"
            Destroy(gameObject);
        }
    }

    private async void StartFlicker()
    {
        if (isFlickering || spriteRenderer == null) return;
        isFlickering = true;

        Color damageColor = Color.red;
        float flickerDuration = 0.1f;
        int flickerCount = 3;

        for (int i = 0; i < flickerCount; i++)
        {
            // Check if object is destroyed
            if (!this || spriteRenderer == null || !isAlive)
            {
                Debug.Log("Object destroyed. Exiting flicker early.");
                break; // Exit flicker logic early
            }

            spriteRenderer.color = damageColor;
            await Task.Delay((int)(flickerDuration * 1000));

            if (!this || spriteRenderer == null || !isAlive)
            {
                Debug.Log("Object destroyed. Exiting flicker early.");
                break; // Exit flicker logic early
            }

            spriteRenderer.color = originalColor;
            await Task.Delay((int)(flickerDuration * 1000));
        }

        isFlickering = false;
    }

    private void OnDrawGizmos()
    {
        if (compositeCollider == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(compositeCollider.bounds.center, compositeCollider.bounds.size);
    }
}
