using UnityEngine;
using UnityEngine.SceneManagement; // To reload the scene

public class PlayerCapyScript : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Shooter shooter; // Reference to the shooter component

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        shooter = GetComponentInChildren<Shooter>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0; // No gravity for the player
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        // Get input for movement (WASD keys)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        // Apply movement to the player
        rb.linearVelocity = movement.normalized * moveSpeed;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Player collided with enemy! Game Over!");
            Die();
        }
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

    public void Die()
    {
        Debug.Log("Player has died!");
        // Option 1: Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        // Option 2: Disable the player sprite
        // gameObject.SetActive(false);

        // Option 3: Show a Game Over screen (handled by GameManager)
        // GameManager.instance.GameOver();
    }
}
