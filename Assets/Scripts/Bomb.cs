using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float speed = 5f; // Speed of the bomb
    public int damage = 20; // Damage dealt in AoE
    public float explosionRadius = 2f; // Explosion radius
    public GameObject explosionEffect; // Optional: explosion visual effect

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Set velocity in the direction the bomb is facing
        rb.linearVelocity = transform.right * speed; // Fixed from `linearVelocity` to `velocity`
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Obstacle"))
        {
            Explode();
        }
    }

    void Explode()
    {
        // Optional explosion effect
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Damage all enemies in the explosion radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                BaseEnemy enemy = collider.GetComponent<BaseEnemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }

        Destroy(gameObject); // Destroy the bomb after it explodes
    }

    void OnDrawGizmosSelected()
    {
        // Draw the radius of the bomb explosion in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject); // Destroy the bomb when it leaves the camera view
    }
}