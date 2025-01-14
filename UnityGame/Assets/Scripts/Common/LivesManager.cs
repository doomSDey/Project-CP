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
        // Skip initialization in GameOver scene
        if (scene.name == "GameOver" || scene.name == "GameFin")
        {
            return;
        }

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
                //Debug.Log("All lives lost!");
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

        // Calculate size of each heart based on LivesPanel
        RectTransform panelRect = livesPanel.GetComponent<RectTransform>();
        if (panelRect == null)
        {
            Debug.LogError("LivesPanel does not have a RectTransform.");
            return;
        }

        float panelWidth = panelRect.rect.width;
        float panelHeight = panelRect.rect.height;

        // Calculate heart size and spacing
        float heartSize = panelWidth / (maxLives + 1); // Adjust spacing as needed
        heartSize = Mathf.Min(heartSize, panelHeight); // Ensure it fits within panel height

        for (int i = 0; i < maxLives; i++)
        {
            GameObject heartObj = new GameObject($"Heart_{i}", typeof(Image));
            heartObj.transform.SetParent(livesPanel, false);

            Image heartImage = heartObj.GetComponent<Image>();
            heartImage.sprite = heartSprite;

            RectTransform rectTransform = heartObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(heartSize, heartSize); // Set size based on calculation
            rectTransform.anchoredPosition = new Vector2((i + 1) * heartSize, 0); // Position hearts horizontally

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
