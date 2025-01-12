using UnityEngine;

public class ScoreOnDestroy : MonoBehaviour
{
    [Header("Score Settings")]
    [SerializeField] private int scoreValue = 10; // Default score value for this object

    private void OnDisable() // Use OnDisable instead of OnDestroy to work with object pooling
    {
        // Check if the game is running and avoid score changes during scene unloading
        if (!Application.isPlaying) return;

        // Add score using the ScoreManager
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }

        // Notify the spawner about the destruction
        PrefabSpawner spawner = FindObjectOfType<PrefabSpawner>();
        if (spawner != null)
        {
            spawner.HandlePrefabDestroyed(gameObject);
        }
    }
}
