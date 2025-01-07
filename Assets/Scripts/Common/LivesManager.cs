using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance;

    [Header("Lives Settings")]
    [SerializeField] private int maxLives = 3;             // Maximum number of lives
    private int currentLives;

    [Header("UI Settings")]
    [SerializeField] private Transform livesPanel;         // Panel to hold heart images
    [SerializeField] private Sprite heartSprite;           // Sprite for heart image

    private List<Image> heartImages = new List<Image>();   // List to store heart UI components

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
        ResetLives();
    }

    public void LoseLife()
    {
        if (currentLives > 0)
        {
            currentLives--;
            UpdateLivesUI();
        }
    }

    public int GetCurrentLives()
    {
        return currentLives;
    }

    public void ResetLives()
    {
        currentLives = maxLives;
        InitializeLivesUI();
    }

    private void InitializeLivesUI()
    {
        // Clear existing hearts
        foreach (Image heart in heartImages)
        {
            Destroy(heart.gameObject);
        }
        heartImages.Clear();

        // Create hearts based on maxLives
        for (int i = 0; i < maxLives; i++)
        {
            GameObject heartObj = new GameObject($"Heart_{i}", typeof(Image));
            heartObj.transform.SetParent(livesPanel, false);

            Image heartImage = heartObj.GetComponent<Image>();
            heartImage.sprite = heartSprite;

            // Explicitly set the size of the heart sprite
            RectTransform rectTransform = heartObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(80, 80); // Adjust to your desired size (e.g., 64x64)

            heartImages.Add(heartImage);
        }

        UpdateLivesUI();
    }

    private void UpdateLivesUI()
    {
        for (int i = 0; i < heartImages.Count; i++)
        {
            // Enable hearts for current lives, disable for lost lives
            heartImages[i].enabled = i < currentLives;
        }
    }
}
