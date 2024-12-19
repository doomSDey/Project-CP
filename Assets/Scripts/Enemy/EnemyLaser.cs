using UnityEngine;

public class EnemyLaser : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 4f;
    public int damage = 20;

    private void Start()
    {
        // Destroy the laser after its lifetime expires
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Move the laser forward in the direction it's facing
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerCapyScript player = collision.gameObject.GetComponent<PlayerCapyScript>();

        Debug.Log("player hit");
        // Damage the player or enemy it collides with
        if (player.CompareTag("Player"))
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
