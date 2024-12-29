using UnityEngine;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance;

    [SerializeField] private int maxLives = 3; // Starting number of lives
    private int currentLives;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Prevent duplicate managers
        }
    }

    private void Start()
    {
        ResetLives(); // Initialize lives
    }

    public void LoseLife()
    {
        if (currentLives > 0)
        {
            currentLives--;
            Debug.Log($"Lives remaining: {currentLives}");
        }
    }

    public int GetCurrentLives()
    {
        return currentLives;
    }

    public void ResetLives()
    {
        currentLives = maxLives;
        Debug.Log("Lives reset to max: " + maxLives);
    }
}
