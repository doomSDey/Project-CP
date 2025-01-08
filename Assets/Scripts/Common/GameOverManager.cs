using UnityEngine.SceneManagement;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public void Retry()
    {
        LivesManager.Instance.ResetLives();
        ScoreManager.Instance.ResetScore();
        SceneManager.LoadScene("Level1");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
