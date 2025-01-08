using UnityEngine;
using UnityEngine.SceneManagement; // For restarting the game
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance;

    [Header("Lives Settings")]
    [SerializeField] private int maxLives = 3; // Maximum number of lives
    private int currentLives;

    [Header("UI Settings")]
    [SerializeField] private Transform livesPanel; // Panel to hold heart images
    [SerializeField] private Sprite heartSprite;   // Sprite for heart image

    private List<Image> heartImages = new List<Image>(); // List to store heart UI components

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeLives();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find the lives panel again after the scene reloads
        livesPanel = GameObject.Find("LivesPanel")?.transform;

        if (livesPanel == null)
        {
            Debug.LogError("LivesPanel not found in the scene.");
            return;
        }

        InitializeLivesUI();
    }

    private void InitializeLives()
    {
        currentLives = maxLives;
        InitializeLivesUI();
    }

    public void LoseLife()
    {
        if (currentLives > 0)
        {
            currentLives--;
            UpdateLivesUI();

            if (currentLives <= 0)
            {
                Debug.Log("All lives lost!");
                StartCoroutine(GoToGameOverScene());
            }
        }
    }

    private IEnumerator GoToGameOverScene()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("GameOver"); // A separate scene
    }

    public void ResetLives()
    {
        currentLives = maxLives;
        UpdateLivesUI();
    }

    private void InitializeLivesUI()
    {
        // Clear old hearts safely
        foreach (Image heart in heartImages)
        {
            if (heart != null)
            {
                Destroy(heart.gameObject);
            }
        }
        heartImages.Clear();

        if (livesPanel == null)
        {
            Debug.LogError("LivesPanel is not assigned.");
            return;
        }

        for (int i = 0; i < maxLives; i++)
        {
            GameObject heartObj = new GameObject($"Heart_{i}", typeof(Image));
            heartObj.transform.SetParent(livesPanel, false);

            Image heartImage = heartObj.GetComponent<Image>();
            heartImage.sprite = heartSprite;

            RectTransform rectTransform = heartObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(80, 80);

            heartImages.Add(heartImage);
        }

        UpdateLivesUI();
    }

    private void UpdateLivesUI()
    {
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (heartImages[i] != null)
            {
                heartImages[i].enabled = i < currentLives;
            }
        }
    }

    public int GetCurrentLives()
    {
        return currentLives;
    }
}
