using UnityEngine;

public class ScoreDrop : MonoBehaviour
{
    private int scoreValue; // Private field to store the score value

    public int ScoreValue // Public property to access the score value
    {
        get => scoreValue;
        set => scoreValue = value;
    }

    public void SetScoreValue(int value)
    {
        scoreValue = value;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("gsde");
        if (collision.CompareTag("Player"))
        {
            // Award the score to the player
            ScoreManager.Instance.AddScore(scoreValue);

            // Destroy this score item after awarding the score
            Destroy(gameObject);
        }
    }
}
