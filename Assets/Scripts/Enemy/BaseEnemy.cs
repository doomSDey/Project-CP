using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public float speed = 2f;
    public int damage = 10;

    protected Rigidbody2D rb;

    protected virtual void Start()
    {
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
    }

    // Each enemy will have its own movement pattern
    protected abstract void Move();

    protected virtual void Update()
    {
        Move();
    }

    public virtual void TakeDamage(int damageAmount)
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
}
