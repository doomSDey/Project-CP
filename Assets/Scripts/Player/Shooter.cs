using UnityEngine;

public class Shooter : MonoBehaviour
{
    public GameObject laserPrefab; // Prefab for laser projectiles
    public GameObject bombPrefab; // Prefab for bomb projectiles
    public Transform firePoint; // Where the projectiles will be instantiated

    public float laserSpeed = 10f; // Speed of the laser
    public float bombSpeed = 5f; // Speed of the bomb

    void Start()
    {
        // If no firePoint is set, use the transform's position as the firePoint
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    void Update()
    {
        // **Rotate the shooter to point towards the mouse**
        RotateTowardsMouse();
    }

    /// <summary>
    /// Rotates the shooter to face the position of the mouse.
    /// </summary>
    void RotateTowardsMouse()
    {
        // Get mouse position in world space
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - (Vector2)transform.position).normalized;

        // Calculate the angle and rotate the shooter
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // Angle in degrees
        transform.rotation = Quaternion.Euler(0, 0, angle); // Rotate the shooter to face the mouse
    }

    /// <summary>
    /// Shoots a laser in the direction the shooter is facing.
    /// </summary>
    public void ShootLaser()
    {
        if (laserPrefab != null)
        {
            // Instantiate laser at firePoint
            GameObject laser = Instantiate(laserPrefab, firePoint.position, firePoint.rotation);

            // Get the Rigidbody2D of the laser
            Rigidbody2D rb = laser.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Fire the laser in the direction of the shooter's current rotation
                Vector2 direction = firePoint.right; // firePoint.right points in the correct direction
                rb.linearVelocity = direction * laserSpeed;
            }
        }
    }

    /// <summary>
    /// Shoots a bomb in the direction the shooter is facing.
    /// </summary>
    public void ShootBomb()
    {
        if (bombPrefab != null)
        {
            // Instantiate bomb at firePoint
            GameObject bomb = Instantiate(bombPrefab, firePoint.position, firePoint.rotation);

            // Get the Rigidbody2D of the bomb
            Rigidbody2D rb = bomb.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Fire the bomb in the direction of the shooter's current rotation
                Vector2 direction = firePoint.right; // firePoint.right points in the correct direction
                rb.linearVelocity = direction * bombSpeed;
            }
        }
    }
}
