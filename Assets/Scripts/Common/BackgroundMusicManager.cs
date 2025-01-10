using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    public AudioSource audioSource;

    private void Awake()
    {
        // Ensure there's only one BackgroundMusicManager in the scene
        DontDestroyOnLoad(gameObject);

        // Get or add an AudioSource component
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    public void PlayMusic()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void PauseMusic()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp(volume, 0f, 1f); // Ensure volume stays within valid range
    }

    public void SetPitch(float pitch)
    {
        audioSource.pitch = Mathf.Clamp(pitch, 0.1f, 3f); // Clamp pitch to prevent extreme values
    }

    public void RandomizePitch(float minPitch = 0.8f, float maxPitch = 1.2f)
    {
        float randomPitch = Random.Range(minPitch, maxPitch);
        SetPitch(randomPitch);
    }
}
