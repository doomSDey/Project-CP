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
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
        rb.freezeRotation = true;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
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
            spriteRenderer.color = hitColor;
            yield return new WaitForSeconds(flickerRate);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flickerRate);

            elapsed += flickerRate * 2;
        }

        spriteRenderer.color = originalColor;
    }

    private IEnumerator DieWithFlicker()
    {
        if (flickerCoroutine != null)
        {
            yield return flickerCoroutine;
        }

        Die();
    }

    protected virtual void Die()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
}
