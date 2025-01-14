using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management

public class SceneTransition : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    [SerializeField] private string sceneName; // Scene name to load, set in the Inspector

    public void LoadScene()
    {
        //Debug.Log("Loading scene: " + sceneName);
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Scene name is not set in the Inspector!");
        }
    }
}
