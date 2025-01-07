using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [SerializeField] private TMP_Text scoreText; // Reference to the Score UI Text
    private int currentScore = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateScoreUI();
    }

    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }

    public int GetScore()
    {
        return currentScore;
    }

    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreUI();
    }
}
