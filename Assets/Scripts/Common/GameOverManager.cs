using UnityEngine.SceneManagement;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public void Retry()
    {
        Debug.Log("Retry button clicked");

        LivesManager.Instance.ResetLives();
        ScoreManager.Instance.ResetScore();
        TimerManager.Instance?.ResetTimer();
        SceneManager.LoadScene("Level1");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
