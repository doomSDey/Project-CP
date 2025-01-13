using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMusicManager : MonoBehaviour
{
    private static BGMusicManager instance;
    private AudioSource audioSource;

    private void Awake()
    {
        // Check if an instance of BGMusicManager already exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Make this object persistent
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to sceneLoaded event
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe to avoid memory leaks
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Mute background music for specific scenes
        if (scene.name == "BossFightCutscene" || scene.name == "StartCutscene")
        {
            MuteMusic(true);
        }
        else
        {
            MuteMusic(false);
        }
    }

    private void MuteMusic(bool mute)
    {
        if (audioSource != null)
        {
            audioSource.mute = mute;
        }
        else
        {
            Debug.LogWarning("No AudioSource component found on BGMusicManager.");
        }
    }
}
