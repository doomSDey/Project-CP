using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public float speed = 2f;
    public int damage = 10;

    private Rigidbody2D rb;

    // Called once on creation
    protected virtual void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
    }

    // Each enemy will have its own movement pattern
    protected abstract void Move();

    // Called every frame
    protected virtual void Update()
    {
        Move();
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log($"{gameObject.name} took {damageAmount} damage! Current Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        Destroy(gameObject); // Destroy the enemy object
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerProjectile"))
        {
            Debug.Log($"{gameObject.name} was hit by {collision.gameObject.name}!");
            TakeDamage(collision.GetComponent<Projectile>().damage);
        }
    }
}
