using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance;

    [Header("Timer Settings")]
    public float levelDuration = 60f;         // Duration in seconds before moving to the next scene
    public string nextSceneName = null; // Name of the next scene to load

    [SerializeField] private TMP_Text timerText; // Reference to the UI Text for displaying the timer

    private float remainingTime;

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to sceneLoaded event
        remainingTime = levelDuration;
        UpdateTimerDisplay();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe from sceneLoaded event
    }

    private void Update()
    {
        remainingTime -= Time.deltaTime;
        UpdateTimerDisplay();

        if (remainingTime <= 0f)
        {
            LoadNextScene();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Skip TimerText updates in GameOver or GameFin scenes
        if (scene.name == "GameOver" || scene.name == "GameFin")
        {
            timerText = null; // Ensure no updates to TimerText
            nextSceneName = null;
            return;
        }

        // Update the TimerText reference for other scenes
        timerText = GameObject.Find("TimerText")?.GetComponent<TMP_Text>();
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        if (remainingTime <= 0f) remainingTime = 0f;

        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);

        if (timerText != null)
        {
            if (remainingTime <= 10f)
            {
                // Exaggerated text for the last 10 seconds
                timerText.text = $"<size=48><color=red>HURRY! {seconds}s</color></size>";
            }
            else
            {
                // Regular timer display
                timerText.text = $"{minutes:00}:{seconds:00}";
            }
        }
    }

    public void ResetTimer()
    {
        remainingTime = levelDuration;
        UpdateTimerDisplay();
    }

    private void LoadNextScene()
    {
        Debug.Log("NExas");
        if (nextSceneName != null)
            SceneManager.LoadScene(nextSceneName);
    }
}
