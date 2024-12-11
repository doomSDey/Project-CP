using UnityEngine;

public class MucusBlob : BaseEnemy
{
    private Vector2 movementDirection;
    private float changeDirectionInterval = 3f;
    private float directionChangeTimer;

    protected override void Start()
    {
        base.Start();
        maxHealth = 20;
        currentHealth = maxHealth;
        speed = 1f;
        movementDirection = GetRandomDirection();
        directionChangeTimer = changeDirectionInterval;
    }

    protected override void Move()
    {
        if (rb == null) return;

        rb.linearVelocity = movementDirection * speed;

        directionChangeTimer -= Time.deltaTime;

        if (directionChangeTimer <= 0f)
        {
            movementDirection = GetRandomDirection();
            directionChangeTimer = changeDirectionInterval;
        }
    }

    private Vector2 GetRandomDirection()
    {
        Vector2[] possibleDirections = { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
        int randomIndex = Random.Range(0, possibleDirections.Length);
        return possibleDirections[randomIndex];
    }
}
