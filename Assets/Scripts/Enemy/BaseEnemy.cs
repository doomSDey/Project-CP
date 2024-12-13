using UnityEngine;
using System.Collections;

public abstract class BaseEnemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public float speed = 2f;
    public int damage = 10;

    [Header("Damage Feedback")]
    [SerializeField] private float flickerDuration = 0.2f;
    [SerializeField] private float flickerRate = 0.1f;
    [SerializeField] private Color hitColor = Color.red;

    protected Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine flickerCoroutine;

    protected virtual void Start()
    {
        // Original initialization
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.bodyType = RigidbodyType2D.Kinematic; // Enemies don't move due to physics forces
        rb.freezeRotation = true;

        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }

        collider.isTrigger = false; // The player should collide with this enemy

        // Initialize sprite renderer for flicker effect
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogWarning("No SpriteRenderer found on enemy!");
        }
    }

    protected abstract void Move();

    protected virtual void Update()
    {
        Move();
    }

    public virtual void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log($"{gameObject.name} took {damageAmount} damage! Current Health: {currentHealth}");

        // Start the flicker effect
        if (spriteRenderer != null)
        {
            if (flickerCoroutine != null)
            {
                StopCoroutine(flickerCoroutine);
            }
            flickerCoroutine = StartCoroutine(FlickerEffect());
        }

        if (currentHealth <= 0)
        {
            StartCoroutine(DieWithFlicker());
        }
    }


    private IEnumerator FlickerEffect()
    {
        float elapsed = 0f;

        while (elapsed < flickerDuration)
        {
            // Toggle between hit color and original color
            spriteRenderer.color = hitColor;
            yield return new WaitForSeconds(flickerRate);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flickerRate);

            elapsed += flickerRate * 2;
        }

        // Ensure we return to the original color
        spriteRenderer.color = originalColor;
    }

    private IEnumerator DieWithFlicker()
    {
        // Wait for any ongoing flicker to complete
        if (flickerCoroutine != null)
        {
            yield return flickerCoroutine;
        }

        Die();
    }
    
    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        // Make sure color is reset before destroying
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log($"{collision.gameObject.name} collided with Player!");
            PlayerCapyScript player = collision.gameObject.GetComponent<PlayerCapyScript>();
            if (player != null)
            {
                player.Die();
            }
        }
    }

    private void OnDestroy()
    {
        // Ensure we reset the color if the object is destroyed mid-flicker
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
}
