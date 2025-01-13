using UnityEngine;

public class BGMusicManager : MonoBehaviour
{
    private static BGMusicManager instance;

    private void Awake()
    {
        // Check if an instance of BGMusicManager already exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Make this object persistent
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }
}
