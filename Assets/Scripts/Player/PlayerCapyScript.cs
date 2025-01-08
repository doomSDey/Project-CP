using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class PlayerCapyScript : MonoBehaviour
{
    // Use baseMoveSpeed to store your original move speed
    public float baseMoveSpeed = 5f;
    public float moveSpeed = 5f;  // This might still be used in other parts of your code
    private float currentMoveSpeed;
    private bool isBeingPushedBack = false; // Flag to track pushback

    private Rigidbody2D rb;
    private Vector2 movement;
    private Shooter shooter;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private float dashTimeLeft;
    private float dashCooldownTimer;
    private bool isDashing;
    private Vector2 dashDirection;

    [Header("Bounce Settings")]
    public float bounceForce = 15f;

    public Tilemap tilemap;
    private Vector3 minBounds;
    private Vector3 maxBounds;
    private Camera mainCamera;
    private Vector3 playerSize;

    public float bombCooldown = 1.5f;
    private float bombCooldownTimer = 0f;

    private SpriteRenderer spriteRenderer;

    // Animator Reference
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        shooter = GetComponentInChildren<Shooter>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;

        // Initialize speeds
        currentMoveSpeed = moveSpeed;
        baseMoveSpeed = moveSpeed;    // Make sure baseMoveSpeed tracks your intended default

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0;

        if (tilemap != null)
        {
            Bounds mapBounds = tilemap.localBounds;
            Vector3 tilemapWorldMin = tilemap.CellToWorld(tilemap.cellBounds.min);
            Vector3 tilemapWorldMax = tilemap.CellToWorld(tilemap.cellBounds.max);

            minBounds = new Vector3(tilemapWorldMin.x, tilemapWorldMin.y, 0);
            maxBounds = new Vector3(tilemapWorldMax.x, tilemapWorldMax.y, 0);
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            playerSize = collider.bounds.extents;
        }
        else
        {
            playerSize = Vector3.zero;
        }

        dashTimeLeft = 0f;
        dashCooldownTimer = 0f;
    }

    void Update()
    {
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (bombCooldownTimer > 0)
        {
            bombCooldownTimer -= Time.deltaTime;
        }

        HandleDash();
        HandleMovement();
        HandleShooting();
        UpdateAnimator();
    }

    private void HandleDash()
    {
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            && dashCooldownTimer <= 0 && !isDashing)
        {
            isDashing = true;
            dashTimeLeft = dashDuration;
            dashCooldownTimer = dashCooldown;

            dashDirection = movement.normalized;
            if (dashDirection == Vector2.zero)
            {
                dashDirection = transform.right;
            }
        }

        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0)
            {
                isDashing = false;
            }
        }
    }

    private void HandleMovement()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Flip the sprite based on horizontal movement
        if (movement.x > 0)
        {
            spriteRenderer.flipX = true; // Facing right
        }
        else if (movement.x < 0)
        {
            spriteRenderer.flipX = false; // Facing left
        }
    }

    private void UpdateAnimator()
    {
        // Update isRunning parameter in the Animator
        bool isRunning = movement != Vector2.zero && !isDashing;
        animator.SetBool("isRunning", isRunning);
    }

    private void FixedUpdate()
    {
        if (isBeingPushedBack) return; // Skip movement logic when being pushed back

        if (isDashing)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
        }
        else
        {
            rb.linearVelocity = movement.normalized * currentMoveSpeed;
        }
    }

    private void LateUpdate()
    {
        CenterCameraOnPlayer();
    }

    private void CenterCameraOnPlayer()
    {
        if (mainCamera == null) return;

        float cameraHalfHeight = mainCamera.orthographicSize;
        float cameraHalfWidth = mainCamera.aspect * cameraHalfHeight;

        Vector3 targetPosition = transform.position;

        float clampedX = Mathf.Clamp(
            targetPosition.x,
            minBounds.x + cameraHalfWidth,
            maxBounds.x - cameraHalfWidth
        );

        float clampedY = Mathf.Clamp(
            targetPosition.y,
            minBounds.y + cameraHalfHeight,
            maxBounds.y - cameraHalfHeight
        );

        mainCamera.transform.position = new Vector3(clampedX, clampedY, mainCamera.transform.position.z);
    }

    void HandleShooting()
    {
        if (isDashing) return;

        if (Input.GetMouseButton(0))
        {
            shooter.ShootLaser();
            // Slight speed penalty while shooting a laser
            currentMoveSpeed = (moveSpeed * 0.6f);
        }
        else if (Input.GetMouseButton(1) && bombCooldownTimer <= 0)
        {
            shooter.ShootBomb();
            // Stop movement temporarily while tossing a bomb
            currentMoveSpeed = 0f;
            bombCooldownTimer = bombCooldown;
        }
        else
        {
            // Return to normal currentMoveSpeed (but still include any MucusBlob penalties)
            currentMoveSpeed = moveSpeed;
        }
    }

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    /// <summary>
    /// Adjust the player's movement speed by an amount (can be positive or negative).
    /// This method clamps the resultant speed so it never goes below zero.
    /// For example, a negative amount can be used to apply a penalty.
    /// </summary>
    /// 

    public void ModifySpeed(float amount)
    {
        float ms = moveSpeed + amount;
        if ((int)moveSpeed > (int)baseMoveSpeed) return;
        moveSpeed = ms;
        if (moveSpeed < 0f) moveSpeed = 0f; // clamp to 0
        Debug.Log($"Speed: {moveSpeed} {baseMoveSpeed} {(int)moveSpeed > (int)baseMoveSpeed}"); // Use string interpolation for logging
        
        // If you're not in some special dash state, also update your currentMoveSpeed
        currentMoveSpeed = moveSpeed;

        Debug.Log($"Player speed changed by {amount}. New moveSpeed = {moveSpeed}");
    }

    public void ApplyPushBack(Vector2 force)
    {
        isBeingPushedBack = true;

        // Apply the pushback force directly to the player's Rigidbody
        rb.linearVelocity = force;

        // Reset the flag after a short delay
        StartCoroutine(ResetPushBack());
    }

    private IEnumerator ResetPushBack()
    {
        yield return new WaitForSeconds(0.2f); // Adjust duration as needed
        isBeingPushedBack = false;
    }

    public void Die()
    {
        Debug.Log("Player has died!");

        // Inform the LivesManager
        LivesManager.Instance.LoseLife();

        if (LivesManager.Instance.GetCurrentLives() > 0)
        {
            StartCoroutine(FlickerEffect());
        }
        else
        {
            Debug.Log("Game Over!");
        }
    }


    private IEnumerator FlickerEffect()
    {
        float flickerDuration = 1f; // Total duration of the flicker effect
        float flickerInterval = 0.1f; // Time between visibility toggles
        float elapsedTime = 0f;

        while (elapsedTime < flickerDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled; // Toggle visibility
            elapsedTime += flickerInterval;
            yield return new WaitForSeconds(flickerInterval);
        }

        spriteRenderer.enabled = true; // Ensure visibility is restored
    }
}
