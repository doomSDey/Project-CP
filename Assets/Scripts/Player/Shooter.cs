using UnityEngine;

public class Shooter : MonoBehaviour
{
    public GameObject laserPrefab;
    public GameObject bombPrefab;
    public Transform firePoint;

    public float laserForce = 500f; // Changed from speed to force
    public float bombForce = 250f;  // Changed from speed to force
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

    public void ShootLaser()
    {
        if (laserPrefab != null)
        {
            GameObject laser = Instantiate(laserPrefab, firePoint.position, firePoint.rotation);
            Rigidbody2D rb = laser.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = firePoint.right;
                // Apply force instead of setting velocity directly
                rb.AddForce(direction * laserForce, ForceMode2D.Impulse);
            }
        }
    }

    public void ShootBomb()
    {
        if (bombPrefab != null)
        {
            GameObject bomb = Instantiate(bombPrefab, firePoint.position, firePoint.rotation);
            Rigidbody2D rb = bomb.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = firePoint.right;
                // Apply force instead of setting velocity directly
                rb.AddForce(direction * bombForce, ForceMode2D.Impulse);
            }
        }
    }
}
