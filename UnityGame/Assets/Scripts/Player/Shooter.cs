using System.Collections;
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
    private bool isShootingLaser = false; // Tracks if laser is being shot

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
        audioSource.loop = true; // Enable looping for laser sound
    }

    void Update()
    {
        SmoothRotateTowardsMouse();
        HandleLaserShooting();
    }

    /// <summary>
    /// Rotates the shooter towards the mouse cursor using smooth rotation.
    /// </summary>
    void SmoothRotateTowardsMouse()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - (Vector2)transform.position).normalized;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, targetAngle);
    }

    /// <summary>
    /// Handles shooting the laser with looping sound.
    /// </summary>
    private void HandleLaserShooting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isShootingLaser = true;
            StartCoroutine(FireLaserStream());
            PlayLaserSound();
        }

        if (Input.GetMouseButtonUp(0))
        {
            isShootingLaser = false;
            StopLaserSound();
        }
    }

    /// <summary>
    /// Shoots a laser projectile continuously while holding the mouse button.
    /// </summary>
    private IEnumerator FireLaserStream()
    {
        while (isShootingLaser)
        {
            ShootLaser();
            yield return new WaitForSeconds(0.1f); // Adjust fire rate as needed
        }
    }

    public void ShootLaser()
    {
        if (laserPrefab != null)
        {
            Vector2 direction = firePoint.right.normalized;
            Vector2 spawnPosition = (Vector2)firePoint.position + direction * bufferDistance;

            GameObject laser = Instantiate(laserPrefab, spawnPosition, firePoint.rotation);
            Rigidbody2D rb = laser.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.AddForce(direction * laserForce, ForceMode2D.Impulse);
            }

            Collider2D playerCollider = transform.parent.GetComponent<Collider2D>();
            Collider2D bulletCollider = laser.GetComponent<Collider2D>();
            if (playerCollider != null && bulletCollider != null)
            {
                Physics2D.IgnoreCollision(playerCollider, bulletCollider);
            }

            Destroy(laser, 2f); // Destroy laser after 2 seconds
        }
    }

    /// <summary>
    /// Plays the laser sound on loop.
    /// </summary>
    private void PlayLaserSound()
    {
        if (laserSound != null && audioSource != null && !audioSource.isPlaying)
        {
            audioSource.clip = laserSound;
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.Play();
        }
    }

    /// <summary>
    /// Stops the laser sound.
    /// </summary>
    private void StopLaserSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void ShootBomb()
    {
        if (bombPrefab != null)
        {
            Vector2 direction = firePoint.right.normalized;
            Vector2 spawnPosition = (Vector2)firePoint.position + direction * bufferDistance;

            GameObject bomb = Instantiate(bombPrefab, spawnPosition, firePoint.rotation);
            Rigidbody2D rb = bomb.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.AddForce(direction * bombForce, ForceMode2D.Impulse);
            }

            PlaySound(bombSound);
            Destroy(bomb, 3f); // Destroy bomb after 3 seconds
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(clip);
        }
    }
}
