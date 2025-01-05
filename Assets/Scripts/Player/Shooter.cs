using UnityEngine;

public class Shooter : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject laserPrefab;
    public GameObject bombPrefab;
    public Transform firePoint;

    [Header("Projectile Forces")]
    public float laserForce = 500f;
    public float bombForce = 250f;
    public float bufferDistance = 0.5f; // Distance from player to bullet start point

    [Header("Rotation Settings")]
    public float rotationSpeed = 10f;

    [Header("Audio Settings")]
    public AudioClip laserSound; // Laser sound effect
    public AudioClip bombSound;  // Bomb sound effect
    public float minPitch = 0.9f; // Minimum pitch for variation
    public float maxPitch = 1.1f; // Maximum pitch for variation

    private AudioSource audioSource; // Shared audio source for both sounds
    private Camera mainCamera;

    void Start()
    {
        if (firePoint == null)
        {
            firePoint = transform;
        }
        mainCamera = Camera.main;

        // Add an AudioSource component to the shooter
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        SmoothRotateTowardsMouse();
    }

    /// <summary>
    /// Rotates the shooter towards the mouse cursor using smooth rotation.
    /// </summary>
    void SmoothRotateTowardsMouse()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - (Vector2)transform.position).normalized;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float currentAngle = transform.eulerAngles.z;
        float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);

        float newAngle = Mathf.MoveTowardsAngle(
            currentAngle,
            currentAngle + angleDiff,
            rotationSpeed * Time.deltaTime * 100
        );

        transform.rotation = Quaternion.Euler(0, 0, newAngle);
    }

    /// <summary>
    /// Shoots a laser projectile from the firePoint.
    /// </summary>
    public void ShootLaser()
    {
        if (laserPrefab != null)
        {
            // Calculate the direction of the shot
            Vector2 direction = firePoint.right.normalized;
            Vector2 spawnPosition = (Vector2)firePoint.position + direction * bufferDistance;

            // Instantiate the laser
            GameObject laser = Instantiate(laserPrefab, spawnPosition, firePoint.rotation);
            Rigidbody2D rb = laser.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.AddForce(direction * laserForce, ForceMode2D.Impulse);
            }

            // Ignore collision with player
            Collider2D playerCollider = transform.parent.GetComponent<Collider2D>();
            Collider2D bulletCollider = laser.GetComponent<Collider2D>();
            if (playerCollider != null && bulletCollider != null)
            {
                Physics2D.IgnoreCollision(playerCollider, bulletCollider);
            }

            // Play laser sound
            PlaySound(laserSound);
        }
    }

    /// <summary>
    /// Shoots a bomb projectile from the firePoint.
    /// </summary>
    public void ShootBomb()
    {
        if (bombPrefab != null)
        {
            // Calculate the direction of the shot
            Vector2 direction = firePoint.right.normalized;
            Vector2 spawnPosition = (Vector2)firePoint.position + direction * bufferDistance;

            // Instantiate the bomb
            GameObject bomb = Instantiate(bombPrefab, spawnPosition, firePoint.rotation);
            Rigidbody2D rb = bomb.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.AddForce(direction * bombForce, ForceMode2D.Impulse);
            }

            // Ignore collision with player
            Collider2D playerCollider = transform.parent.GetComponent<Collider2D>();
            Collider2D bulletCollider = bomb.GetComponent<Collider2D>();
            if (playerCollider != null && bulletCollider != null)
            {
                Physics2D.IgnoreCollision(playerCollider, bulletCollider);
            }

            // Play bomb sound
            PlaySound(bombSound);
        }
    }

    /// <summary>
    /// Plays the specified sound with optional pitch variation.
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.pitch = Random.Range(minPitch, maxPitch); // Apply pitch variation
            audioSource.Play();
        }
    }
}
