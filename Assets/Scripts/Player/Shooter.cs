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

    private Camera mainCamera;

    void Start()
    {
        if (firePoint == null)
        {
            firePoint = transform;
        }
        mainCamera = Camera.main;
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
            // 1️⃣ Calculate the direction of the shot
            Vector2 direction = firePoint.right.normalized;

            // 2️⃣ Calculate the spawn position of the laser
            Vector2 spawnPosition = (Vector2)firePoint.position + direction * bufferDistance;

            // 3️⃣ Instantiate the laser at the calculated position
            GameObject laser = Instantiate(laserPrefab, spawnPosition, firePoint.rotation);

            // 4️⃣ Get the Rigidbody of the laser
            Rigidbody2D rb = laser.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                // 5️⃣ Apply force to the laser
                rb.AddForce(direction * laserForce, ForceMode2D.Impulse);
            }

            // 6️⃣ Ignore collision with player
            Collider2D playerCollider = transform.parent.GetComponent<Collider2D>();
            Collider2D bulletCollider = laser.GetComponent<Collider2D>();
            if (playerCollider != null && bulletCollider != null)
            {
                Physics2D.IgnoreCollision(playerCollider, bulletCollider);
            }
        }
    }

    /// <summary>
    /// Shoots a bomb projectile from the firePoint.
    /// </summary>
    public void ShootBomb()
    {
        if (bombPrefab != null)
        {
            // 1️⃣ Calculate the direction of the shot
            Vector2 direction = firePoint.right.normalized;

            // 2️⃣ Calculate the spawn position of the bomb
            Vector2 spawnPosition = (Vector2)firePoint.position + direction * bufferDistance;

            // 3️⃣ Instantiate the bomb at the calculated position
            GameObject bomb = Instantiate(bombPrefab, spawnPosition, firePoint.rotation);

            // 4️⃣ Get the Rigidbody of the bomb
            Rigidbody2D rb = bomb.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                // 5️⃣ Apply force to the bomb
                rb.AddForce(direction * bombForce, ForceMode2D.Impulse);
            }

            // 6️⃣ Ignore collision with player
            Collider2D playerCollider = transform.parent.GetComponent<Collider2D>();
            Collider2D bulletCollider = bomb.GetComponent<Collider2D>();
            if (playerCollider != null && bulletCollider != null)
            {
                Physics2D.IgnoreCollision(playerCollider, bulletCollider);
            }
        }
    }
}
