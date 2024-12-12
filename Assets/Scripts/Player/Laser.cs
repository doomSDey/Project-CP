using UnityEngine;

public class Laser : MonoBehaviour
{
    public float speed = 10f; // Speed of the laser
    public int damage = 10; // Damage dealt by the laser

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Set velocity in the direction the laser is facing
        rb.linearVelocity = transform.right * speed; // Fixed from `linearVelocity` to `velocity`
    }

    void OnCollisionEnter2D(Collision2D hitInfo)
    {
        // If the laser hits an enemy, deal damage and destroy the laser
        if (hitInfo.gameObject.CompareTag("Enemy"))
        {
            BaseEnemy enemy = hitInfo.gameObject.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject); // Destroy the laser
        }

        // If the laser hits an obstacle, it is destroyed
        if (hitInfo.gameObject.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject); // Destroy the laser when it leaves the camera view
    }
}
