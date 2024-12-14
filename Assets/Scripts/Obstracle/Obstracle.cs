using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [Header("Bounce Settings")]
    [SerializeField] private bool canBounceBack = true;
    [SerializeField] private float bounceInterval = 0.5f;
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
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        currentHealth = maxHealth;
        bounceTimer = 0f;
    }

    private void Update()
    {
        if (bounceTimer > 0)
        {
            bounceTimer -= Time.deltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!canBounceBack) return;

        if (collision.gameObject.CompareTag("Player") && bounceTimer <= 0)
        {
            PlayerCapyScript player = collision.gameObject.GetComponent<PlayerCapyScript>();
            if (player != null)
            {
                player.BounceBack(collision.contacts[0].point);
                bounceTimer = bounceInterval;
            }
        }
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
            await System.Threading.Tasks.Task.Delay((int)(flickerDuration * 1000));
            spriteRenderer.color = originalColor;
            await System.Threading.Tasks.Task.Delay((int)(flickerDuration * 1000));
        }

        isFlickering = false;
    }
}
