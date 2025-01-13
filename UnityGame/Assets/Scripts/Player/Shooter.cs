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
    public float bufferDistance = 0.5f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 10f;

    [Header("Audio Settings")]
    public AudioClip laserSound;
    public AudioClip bombSound;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    private AudioSource audioSource;
    private Camera mainCamera;
    private bool isShootingLaser = false;

    void Start()
    {
        if (firePoint == null)
        {
            firePoint = transform;
        }

        mainCamera = Camera.main;

        // Ensure the camera is at the correct Z position
        if (mainCamera.transform.position.z != -10f)
        {
            mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, -10f);
        }

        // Setup AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true; // Enable looping for laser sound
    }

    void Update()
    {
        HandleLaserShooting();
    }

    void FixedUpdate()
    {
        SmoothRotateTowardsMouse();
    }

    /// <summary>
    /// Rotates the shooter smoothly towards the mouse position.
    /// </summary>
    void SmoothRotateTowardsMouse()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // Ensure 2D rotation

        Vector3 direction = (mousePosition - transform.position).normalized;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Handles laser shooting input and sound.
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

    private void PlayLaserSound()
    {
        if (laserSound != null && audioSource != null && !audioSource.isPlaying)
        {
            audioSource.clip = laserSound;
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.Play();
        }
    }

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
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(clip);
        }
    }
}
