using UnityEngine;

public class Laser : MonoBehaviour
{
    public float speed = 10f; // Speed of the laser
    public int damage = 10; // Damage dealt by the laser
    public AudioClip laserFireSound; // Laser fire sound effect
    public float minPitch = 0.9f; // Minimum pitch for variation
    public float maxPitch = 1.1f; // Maximum pitch for variation

    private Rigidbody2D rb;
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Set velocity in the direction the laser is facing
        rb.linearVelocity = transform.right * speed;

        // Add an AudioSource component to play the sound
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = laserFireSound;
        audioSource.playOnAwake = false;

        // Reduce volume by 50%
        audioSource.volume = 0.1f;

        // Apply pitch variation
        audioSource.pitch = Random.Range(minPitch, maxPitch);

        // Play the laser fire sound
        audioSource.Play();
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
            Obstacle obstacle = hitInfo.gameObject.GetComponent<Obstacle>();
            if (obstacle != null)
                obstacle.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject); // Destroy the laser when it leaves the camera view
    }
}
