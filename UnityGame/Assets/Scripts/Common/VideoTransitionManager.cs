using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TimestampPauseManager : MonoBehaviour
{
    [Header("Video Setup")]
    [Tooltip("Reference to the VideoPlayer in the scene.")]
    public VideoPlayer videoPlayer;

    [Tooltip("List of times (in seconds) at which the video will pause. Must be in ascending order.")]
    public List<float> pauseTimestamps;

    [Header("UI Elements")]
    [Tooltip("Button shown when video pauses or ends. Click to continue.")]
    public Button nextButton;

    [Tooltip("Name of the scene to load after the video ends.")]
    public string nextSceneName;

    private int currentPauseIndex = 0;
    private bool isPausedManually = false;
    private bool isVideoEnded = false;

    private void Start()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("No VideoPlayer assigned to TimestampPauseManager!");
            return;
        }

        if (pauseTimestamps == null || pauseTimestamps.Count == 0)
        {
            Debug.LogWarning("No pause timestamps assigned. Video will only stop at the end.");
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(false);
            nextButton.onClick.AddListener(OnNextButtonClick);
        }
        else
        {
            Debug.LogWarning("No Button assigned. Video will pause but cannot be resumed via UI.");
        }

        videoPlayer.loopPointReached += OnVideoEnd; // Attach event for video end
        videoPlayer.Play();
    }

    private void Update()
    {
        if (isVideoEnded || currentPauseIndex >= pauseTimestamps.Count || isPausedManually)
            return;

        if (videoPlayer.time >= pauseTimestamps[currentPauseIndex])
        {
            videoPlayer.Pause();
            isPausedManually = true;

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(true);
            }

            Debug.Log($"Video paused at {videoPlayer.time:F2}s. Waiting for user input.");
        }
    }

    private void OnNextButtonClick()
    {
        if (isVideoEnded)
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            nextButton.gameObject.SetActive(false);
            isPausedManually = false;

            currentPauseIndex++;
            videoPlayer.Play();

            Debug.Log($"User resumed video. Next pause index = {currentPauseIndex} (time = {videoPlayer.time:F2}s).");
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        isVideoEnded = true;
        nextButton.gameObject.SetActive(true);

        Debug.Log("Video finished playing. Showing transition button.");
    }
}
