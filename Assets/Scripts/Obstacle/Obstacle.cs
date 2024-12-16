using UnityEngine;
using System.Threading.Tasks;  // Needed for async flicker logic

public class Obstacle : MonoBehaviour
{
    [Header("Bounce Settings")]
    [SerializeField] private bool canBounceBack = true;
    [SerializeField] private float bounceInterval = 3f; // Interval to apply push back (every 3 seconds)
    [SerializeField] private float pushBackForce = 100f; // Force to push back objects
    private float bounceTimer;

    [Header("Health Settings")]
    [SerializeField] private bool isDestroyable = false;
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isFlickering = false;

    private void Start()
    {
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
        bounceTimer = 0f; // Timer for push-back logic
    }

    private void Update()
    {
        if (bounceTimer > 0)
        {
            bounceTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// This is called on every physics update to check for player collision.
    /// </summary>
    private void FixedUpdate()
    {
        if (!canBounceBack) return;

        // Check for collisions using OverlapBox (size of the object)
        Collider2D playerCollider = Physics2D.OverlapBox(
            transform.position,
            GetComponent<BoxCollider2D>().size,
            0f,
            LayerMask.GetMask("Player") // Ensure the Player is on the Player layer
        );

        if (playerCollider != null && bounceTimer <= 0)
        {
            Rigidbody2D playerRb = playerCollider.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                PushBack(playerRb);
                bounceTimer = bounceInterval; // Reset the bounce timer
            }
        }
    }

    /// <summary>
    /// Push the player back.
    /// </summary>
    private void PushBack(Rigidbody2D playerRb)
    {
        if (playerRb == null) return;

        // Reset player velocity to make sure push works
        playerRb.linearVelocity = Vector2.zero;

        // Calculate push direction from player to the obstacle
        Vector2 direction = (playerRb.position - (Vector2)transform.position).normalized;

        // Apply the force to push the player back
        playerRb.AddForce(direction * pushBackForce, ForceMode2D.Impulse);
        Debug.Log($"Pushing player back in direction {direction}");
    }

    public void TakeDamage(int damage)
    {
        if (!isDestroyable) return;

        currentHealth -= damage;
        StartFlicker();

        if (currentHealth <= 0)
        {
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
            spriteRenderer.color = damageColor;
            await Task.Delay((int)(flickerDuration * 1000));
            spriteRenderer.color = originalColor;
            await Task.Delay((int)(flickerDuration * 1000));
        }

        isFlickering = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider2D>().size);
    }
}
