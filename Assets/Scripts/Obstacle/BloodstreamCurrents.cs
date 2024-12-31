using UnityEngine;

public class DriftZone : MonoBehaviour
{
    [Header("Force Settings")]
    [SerializeField] private Vector2 forceDirection = new Vector2(1, 0); // Default force direction (X-axis)
    [SerializeField] private float forceMagnitude = 5f; // The strength of the force
    [SerializeField] private bool affectAllRigidbodies = false; // Apply force to all Rigidbody2D, not just the player

    private void OnTriggerStay2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Check if we only want to affect the player
            if (!affectAllRigidbodies && !collision.CompareTag("Player")) return;

            // Normalize the direction to ensure consistent force
            Vector2 appliedForce = forceDirection.normalized * forceMagnitude;

            // Apply the force to the Rigidbody2D
            rb.AddForce(appliedForce, ForceMode2D.Force);
        }
    }

    private void OnDrawGizmos()
    {
        // Draw the force direction in the Scene view for better debugging
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)forceDirection.normalized * 2);
    }
}
