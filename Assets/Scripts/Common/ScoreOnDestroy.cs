using UnityEngine;

public class ScoreOnDestroy : MonoBehaviour
{
    [Header("Score Settings")]
    [SerializeField] private int scoreValue = 10; // Default score value for this object

    private void OnDestroy()
    {
        // Check if the game is running and avoid score changes during scene unloading
        if (!Application.isPlaying) return;

        // Add score using the ScoreManager
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }
    }
}
