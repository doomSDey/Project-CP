using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCapyScript : MonoBehaviour
{
    public float baseMoveSpeed = 5f;
    public float moveSpeed = 5f;
    private float currentMoveSpeed;
    private bool isBeingPushedBack = false;

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

    public float bombCooldown = 1.5f;
    private float bombCooldownTimer = 0f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        shooter = GetComponentInChildren<Shooter>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Initialize speeds
        currentMoveSpeed = moveSpeed;
        baseMoveSpeed = moveSpeed;

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0;

        dashTimeLeft = 0f;
        dashCooldownTimer = 0f;
    }

    private void Update()
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
        HandleMovementAudio(); // Play audio based on movement
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

    private void HandleMovementAudio()
    {
        if (movement != Vector2.zero) // Player is moving
        {
            if (!audioSource.isPlaying)
            {
                audioSource.pitch = Random.Range(minPitch, maxPitch); // Apply pitch variation
                audioSource.Play();
            }
        }
        else // Player stopped moving
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    private void UpdateAnimator()
    {
        bool isRunning = movement != Vector2.zero && !isDashing;
        animator.SetBool("isRunning", isRunning);
    }

    private void FixedUpdate()
    {
        if (isBeingPushedBack) return;

        if (isDashing)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
        }
        else
        {
            rb.linearVelocity = movement.normalized * currentMoveSpeed;
        }
    }

    void HandleShooting()
    {
        if (isDashing) return;

        if (Input.GetMouseButton(0))
        {
            shooter.ShootLaser();
            currentMoveSpeed = (moveSpeed * 0.6f);
        }
        else if (Input.GetMouseButton(1) && bombCooldownTimer <= 0)
        {
            shooter.ShootBomb();
            currentMoveSpeed = 0f;
            bombCooldownTimer = bombCooldown;
        }
        else
        {
            currentMoveSpeed = moveSpeed;
        }
    }

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    public void ModifySpeed(float amount)
    {
        float ms = moveSpeed + amount;
        if (moveSpeed > baseMoveSpeed) return;
        moveSpeed = ms;
        if (moveSpeed < 0f) moveSpeed = 0f;

        currentMoveSpeed = moveSpeed;
        Debug.Log($"Player speed changed by {amount}. New moveSpeed = {moveSpeed}");
    }

    public void ApplyPushBack(Vector2 force)
    {
        isBeingPushedBack = true;
        rb.linearVelocity = force;
        StartCoroutine(ResetPushBack());
    }

    private IEnumerator ResetPushBack()
    {
        yield return new WaitForSeconds(0.2f);
        isBeingPushedBack = false;
    }

    public void Die()
    {
        Debug.Log("Player has died!");
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
        float flickerDuration = 1f;
        float flickerInterval = 0.1f;
        float elapsedTime = 0f;

        while (elapsedTime < flickerDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            elapsedTime += flickerInterval;
            yield return new WaitForSeconds(flickerInterval);
        }

        spriteRenderer.enabled = true;
    }
}
