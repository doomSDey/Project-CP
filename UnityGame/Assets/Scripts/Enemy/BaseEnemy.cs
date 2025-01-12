using System;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    protected float maxHealth = 100f;
    protected float currentHealth;
    protected float speed = 5f;
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    protected Color originalColor;
    public event Action<GameObject> OnDestroyed;

    [SerializeField] protected float damageFlashDuration = 0.1f;
    [SerializeField] protected Color damageFlashColor = Color.red;
    protected bool isFlashing = false;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        currentHealth = maxHealth;
    }

    protected virtual void Update()
    {
        Move();
    }

    protected virtual void Move()
    {
        // Base movement behavior - override in derived classes
    }

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");

        StartDamageFlash();

        if (currentHealth <= 0)
        {
            // Wait for the flash to complete before "dying"
            Invoke("Die", damageFlashDuration);
        }
    }

    public virtual void Die()
    {
        Debug.Log($"{gameObject.name} has died!");

        // Notify the spawner and deactivate the object
        OnDestroyed?.Invoke(gameObject);

        // Reset health for future reuse
        currentHealth = maxHealth;

        // Deactivate the enemy instead of destroying it
        gameObject.SetActive(false);
    }

    protected void StartDamageFlash()
    {
        if (!isFlashing && spriteRenderer != null)
        {
            isFlashing = true;
            spriteRenderer.color = damageFlashColor;
            Invoke("EndDamageFlash", damageFlashDuration);
        }
    }

    protected void EndDamageFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        isFlashing = false;
    }
}
