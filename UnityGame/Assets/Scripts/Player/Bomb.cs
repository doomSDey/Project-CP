using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float speed = 5f; // Speed of the bomb
    public int damage = 20; // Damage dealt in AoE
    public float explosionRadius = 2f; // Explosion radius

    [SerializeField] private float shakeIntensity = 0.5f; // Value between 0 and 1
    private CameraShake cameraShake;
    private Rigidbody2D rb;
    private Animator animator; // Reference to the Animator
    private bool hasExploded = false; // Ensure Explode is called only once

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Set velocity in the direction the bomb is facing
        rb.linearVelocity = transform.right * speed;

        cameraShake = Camera.main.GetComponent<CameraShake>();
        if (cameraShake == null)
        {
            Debug.LogWarning("CameraShake component not found on Main Camera!");
        }

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("Animator component not found on Bomb!");
        }

        // Ignore collisions with the player
        Collider2D playerCollider = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), playerCollider);
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!hasExploded && (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Obstacle")))
        {
            hasExploded = true;
            Explode();
        }
    }

    void Explode()
    {
        Debug.Log("Explode triggered");

        // Trigger camera shake
        if (cameraShake != null)
        {
            cameraShake.AddTrauma(shakeIntensity);
        }

        // Trigger explosion animation
        if (animator != null)
        {
            animator.SetTrigger("Explode");
        }

        // Damage all enemies and obstacles in the explosion radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                BaseEnemy enemy = collider.GetComponent<BaseEnemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
            else if (collider.CompareTag("Obstacle"))
            {
                Obstacle obstacle = collider.GetComponent<Obstacle>();
                if (obstacle != null)
                {
                    obstacle.TakeDamage(damage);
                }
            }
        }

        // Destroy the bomb after the animation finishes
        StartCoroutine(DestroyAfterAnimation());
    }

    private IEnumerator DestroyAfterAnimation()
    {
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // Wait for the animation to complete before destroying the object
            while (stateInfo.IsName("Explosion") && stateInfo.normalizedTime < 5.0f)
            {
                yield return null; // Wait until the animation is fully played
                stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            }
        }

        Destroy(gameObject); // Destroy the bomb after the animation finishes
    }

    void OnDrawGizmosSelected()
    {
        // Draw the radius of the bomb explosion in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject); // Destroy the bomb when it leaves the camera view
    }
}
