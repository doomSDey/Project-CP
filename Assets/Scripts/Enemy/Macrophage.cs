using UnityEngine;

public class Macrophage : BaseEnemy
{
    [Header("Chase Settings")]
    [SerializeField] private float chaseRange = 10f; // Range to detect and chase player
    [SerializeField] private float stunDuration = 2f; // Duration of stun when hit by Capy Bomb
    private GameObject player; // Reference to the player
    private bool isStunned = false; // Tracks if the macrophage is stunned

    protected override void Start()
    {
        base.Start();

        // Ensure Rigidbody is Kinematic so we have full control of movement
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Locate the player object (player should be tagged as "Player" in Unity)
        player = GameObject.FindGameObjectWithTag("Player");

        // Macrophage is unkillable, so remove health properties
        maxHealth = Mathf.Infinity; // Infinite health
        currentHealth = maxHealth;
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void Move()
    {
        if (isStunned) return; // If stunned, do not move

        if (player == null) return;

        // Check if player is within chase range
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= chaseRange)
        {
            // Chase the player
            Vector2 targetPosition = player.transform.position;
            Vector2 newPosition = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If the Macrophage touches the player, the player dies instantly
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player touched Macrophage — Instantly Dead!");
            PlayerCapyScript player = collision.gameObject.GetComponent<PlayerCapyScript>();
            if (player != null)
            {
                player.Die();
            }
        }
        // If the Macrophage is hit by a Capy Bomb, it gets stunned
        else if (collision.gameObject.CompareTag("Bomb"))
        {
            Debug.Log("Macrophage hit by Capy Bomb — Stunned for " + stunDuration + " seconds!");
            Stun();
        }
    }

    /// <summary>
    /// Stuns the Macrophage, making it unable to move for a set duration.
    /// </summary>
    public void Stun()
    {
        if (!isStunned)
        {
            StartCoroutine(ApplyStun());
        }
    }

    /// <summary>
    /// Coroutine that stuns the macrophage for a certain duration.
    /// </summary>
    private System.Collections.IEnumerator ApplyStun()
    {
        isStunned = true;

        // Change color to visually indicate stun
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.blue; // Indicate that the Macrophage is stunned
        }

        // Wait for stun duration
        yield return new WaitForSeconds(stunDuration);

        // Restore movement and reset color
        isStunned = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor; // Revert color back to normal
        }
    }

    /// <summary>
    /// Macrophage is unkillable, so override TakeDamage to do nothing.
    /// </summary>
    public override void TakeDamage(float damage)
    {
        Debug.Log($"{gameObject.name} is unkillable. Ignoring damage.");
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a red wireframe circle in the Unity Editor to visualize the chase range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}
