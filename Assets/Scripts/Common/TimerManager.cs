using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TimerManager : MonoBehaviour
{
    [Header("Timer Settings")]
    public float levelDuration = 60f; // Duration in seconds before moving to the next scene
    public TMP_Text timerText;        // Reference to the UI Text for displaying the timer
    public string nextSceneName = "NextScene"; // Name of the next scene to load

    private float remainingTime;

    private void Start()
    {
        remainingTime = levelDuration;
        UpdateTimerDisplay();
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

    private void UpdateTimerDisplay()
    {
        if (remainingTime <= 0f) remainingTime = 0f;

        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);

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

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
