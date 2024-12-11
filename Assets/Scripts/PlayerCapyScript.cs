using UnityEngine;

public class PlayerCapyScript : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Shooter shooter; // Reference to the shooter component

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Automatically find the Shooter component on a child object
        shooter = GetComponentInChildren<Shooter>();

        if (shooter == null)
        {
            Debug.LogError("Shooter not found! Please make sure the Shooter is a child of PlayerCapyScript.");
        }
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();
    }

    void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector2 movement = new Vector2(horizontalInput, verticalInput);
        rb.linearVelocity = movement * moveSpeed;
    }

    void HandleShooting()
    {
        if (Input.GetMouseButton(0)) // Left click to shoot laser
        {
            shooter.ShootLaser();
        }

        if (Input.GetMouseButtonDown(1)) // Right click to shoot bomb
        {
            shooter.ShootBomb();
        }
    }
}
