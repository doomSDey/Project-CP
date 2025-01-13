using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SeamlessVideoManager : MonoBehaviour
{
    [Header("Video Players")]
    public VideoPlayer videoPlayerA; // First VideoPlayer
    public VideoPlayer videoPlayerB; // Second VideoPlayer

    [Header("Video Clips")]
    public VideoClip[] videoClips; // Array of video clips
    private int currentClipIndex = 0; // Track the current clip index

    [Header("UI Elements")]
    public Button nextButton; // Button to transition to the next video
    public string nextSceneName; // Name of the scene to load after the last video

    private bool isPlayerAPlaying = true; // Track which VideoPlayer is active

    private void Start()
    {
        if (videoClips.Length > 0)
        {
            videoPlayerA.clip = videoClips[currentClipIndex];
            videoPlayerA.Play();
            PrepareNextClip(videoPlayerB);

            nextButton.gameObject.SetActive(false); // Hide button initially
            nextButton.onClick.AddListener(OnNextButtonClick); // Add button click event
        }
    }

    private void Update()
    {
        // Show the button when the current video finishes playing
        if (isPlayerAPlaying && videoPlayerA.time >= videoPlayerA.clip.length - 0.1f && !nextButton.gameObject.activeSelf)
        {
            nextButton.gameObject.SetActive(true);
        }
        else if (!isPlayerAPlaying && videoPlayerB.time >= videoPlayerB.clip.length - 0.1f && !nextButton.gameObject.activeSelf)
        {
            nextButton.gameObject.SetActive(true);
        }
    }

    private void OnNextButtonClick()
    {
        nextButton.gameObject.SetActive(false); // Hide the button

        if (currentClipIndex + 1 < videoClips.Length)
        {
            // Play the next video
            currentClipIndex++;
            PlayNextVideo(isPlayerAPlaying ? videoPlayerB : videoPlayerA);
        }
        else
        {
            // If all videos are finished, load the next scene
            Debug.Log("All videos played. Loading next scene...");
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void PlayNextVideo(VideoPlayer nextPlayer)
    {
        nextPlayer.Play(); // Start playing the next video
        PrepareNextClip(isPlayerAPlaying ? videoPlayerA : videoPlayerB); // Prepare the next clip
        isPlayerAPlaying = !isPlayerAPlaying; // Toggle which player is active
    }

    private void PrepareNextClip(VideoPlayer nextPlayer)
    {
        int nextClipIndex = (currentClipIndex + 1) % videoClips.Length;
        nextPlayer.clip = videoClips[nextClipIndex];
        nextPlayer.Prepare(); // Preload the next video to avoid any lag
    }
}
