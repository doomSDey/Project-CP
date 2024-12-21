using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 5f;
    public int damage = 10;

    private void Start()
    {
        // Destroy the bullet after its lifetime expires
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Move the bullet forward
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerCapyScript player = collision.gameObject.GetComponent<PlayerCapyScript>();

        Debug.Log("player hit");
        // Damage the player or enemy it collides with
        if (collision.gameObject.CompareTag("Player"))
        {
            if (player != null)
            {
                player.Die();
                Destroy(gameObject);
            }
        }

        // Destroy bullet on impact
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
