using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Video;

[System.Serializable]
public class AudioClipData
{
    public AudioClip clip;        // The audio clip to play
    public float startTime;       // Start timestamp in seconds
    public float endTime;         // End timestamp in seconds
}

public class AudioSFXManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource backgroundMusicSource; // AudioSource for background music
    public AudioSource soundEffectSource;     // AudioSource for sound effects (clips)
    public VideoPlayer videoPlayer;           // VideoPlayer to sync audio timestamps with
    public List<AudioClipData> audioClipDataList; // List of audio clip data

    private void Start()
    {
        // Ensure the background music source is set up
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.loop = true; // Loop the background music
            backgroundMusicSource.Play();
        }

        // Play the audio clips based on timestamps
        StartCoroutine(PlayAudioClips());
    }

    private IEnumerator PlayAudioClips()
    {
        foreach (var audioClipData in audioClipDataList)
        {
            while (videoPlayer.time < audioClipData.startTime)
            {
                yield return null; // Wait until the video reaches the clip's start time
            }

            if (audioClipData.endTime > 0)
            {
                float clipDuration = audioClipData.endTime - audioClipData.startTime;
                StartCoroutine(FadeBackgroundMusic(0.2f, clipDuration));
                PlayClip(audioClipData);
                yield return new WaitForSeconds(clipDuration);
            }
            else
            {
                PlayClip(audioClipData);
            }
        }
    }

    private void PlayClip(AudioClipData audioClipData)
    {
        if (soundEffectSource != null && audioClipData.clip != null)
        {
            soundEffectSource.clip = audioClipData.clip;
            soundEffectSource.volume = 0.74f; // Match the volume setting in the screenshot
            soundEffectSource.spatialBlend = 0f; // Ensure it's 2D audio
            soundEffectSource.priority = 186; // Match the priority setting
            soundEffectSource.Play();

            if (audioClipData.endTime > 0)
            {
                StartCoroutine(LoopClipUntilEndTime(audioClipData));
            }
        }
    }

    private IEnumerator LoopClipUntilEndTime(AudioClipData audioClipData)
    {
        while (videoPlayer.time < audioClipData.endTime)
        {
            soundEffectSource.Play();
            yield return new WaitForSeconds(soundEffectSource.clip.length);
        }
    }

    private IEnumerator FadeBackgroundMusic(float targetVolume, float duration)
    {
        float originalVolume = backgroundMusicSource.volume;
        float fadeOutSpeed = (originalVolume - targetVolume) / (duration / 2);
        float fadeInSpeed = fadeOutSpeed;

        // Fade out
        while (backgroundMusicSource.volume > targetVolume)
        {
            backgroundMusicSource.volume -= fadeOutSpeed * Time.deltaTime;
            yield return null;
        }

        backgroundMusicSource.volume = targetVolume;

        // Wait for the duration of the sound effect
        yield return new WaitForSeconds(duration);

        // Fade in
        while (backgroundMusicSource.volume < originalVolume)
        {
            backgroundMusicSource.volume += fadeInSpeed * Time.deltaTime;
            yield return null;
        }

        backgroundMusicSource.volume = originalVolume; // Restore original volume
    }
}
