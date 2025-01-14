using UnityEngine;

public class MucusBlob : BaseEnemy
{
    [Header("Movement Settings")]
    [SerializeField] private float changeDirectionInterval = 5f;
    private Vector2 movementDirection;
    private float directionChangeTimer;

    [Header("Physics Settings")]
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Attachment Settings")]
    [SerializeField] private float speedReductionAmount = 1f;

    private bool isAttached = false;
    private PlayerCapyScript attachedPlayer;

    protected override void Start()
    {
        base.Start();

        // Set specific stats for the Mucus Blob
        currentHealth = 20;
        speed = 1f;
        movementDirection = GetRandomDirection();
        directionChangeTimer = changeDirectionInterval;

        // Subscribe to OnDestroyed so we can restore speed if we die
        OnDestroyed += HandleBlobDestroyed;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void HandleBlobDestroyed(GameObject blobGameObject)
    {
        // If this blob is destroyed and we were attached, restore speed
        if (blobGameObject == gameObject && isAttached && attachedPlayer != null)
        {
            attachedPlayer.ModifySpeed(speedReductionAmount);
            //Debug.Log($"{name} destroyed; speed restored by {speedReductionAmount}.");
        }
    }

    protected override void Move()
    {
        if (isAttached || rb == null) return;

        Vector2 newPosition = rb.position + movementDirection * speed * Time.fixedDeltaTime;

        RaycastHit2D hit = Physics2D.Raycast(
            rb.position,
            movementDirection,
            speed * Time.fixedDeltaTime,
            obstacleLayer
        );
        if (hit.collider != null)
        {
            BounceBack(hit.normal);
        }
        else
        {
            rb.MovePosition(newPosition);
        }

        directionChangeTimer -= Time.deltaTime;
        if (directionChangeTimer <= 0f)
        {
            movementDirection = GetRandomDirection();
            directionChangeTimer = changeDirectionInterval;
        }
    }

    private Vector2 GetRandomDirection()
    {
        Vector2[] possibleDirections = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        int randomIndex = Random.Range(0, possibleDirections.Length);
        return possibleDirections[randomIndex];
    }

    private void BounceBack(Vector2 collisionNormal)
    {
        movementDirection = Vector2.Reflect(movementDirection, collisionNormal).normalized;
        //Debug.Log($"{name} bounced back after hitting {collisionNormal}");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If we hit Player and not attached yet, attach
        if (!isAttached && collision.gameObject.CompareTag("Player"))
        {
            AttachToPlayer(collision.gameObject);
        }
    }

    private void AttachToPlayer(GameObject playerObject)
    {
        attachedPlayer = playerObject.GetComponent<PlayerCapyScript>();
        if (attachedPlayer == null)
        {
            Debug.LogWarning($"{name} collided with Player, but no PlayerCapyScript found!");
            return;
        }

        // Parent to the player so we visually follow
        transform.SetParent(playerObject.transform);

        // Make the blob kinematic so it won't fight the player's movement
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        // Ignore collisions with the Player
        Collider2D playerCollider = playerObject.GetComponent<Collider2D>();
        Collider2D blobCollider = GetComponent<Collider2D>();
        if (playerCollider && blobCollider)
        {
            Physics2D.IgnoreCollision(playerCollider, blobCollider, true);
        }

        isAttached = true;

        // Apply speed reduction (subtract)
        attachedPlayer.ModifySpeed(-speedReductionAmount);
        //Debug.Log($"{name} attached to Player. Speed reduced by {speedReductionAmount}.");
    }

  
}
