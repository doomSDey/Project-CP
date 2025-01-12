using UnityEngine;
using System.Collections.Generic;

public class ScoreItemSpawner : MonoBehaviour
{
    [Header("Score Item Settings")]
    public GameObject scoreDropsPrefab; // Prefab containing a SpriteRenderer
    public int numberOfScoreItems = 10;
    public LayerMask obstacleLayer;
    public float spawnCheckRadius = 0.5f;
    public float spawnInterval = 5f;
    public Transform player; // Reference to the player's transform

    [Header("Sprites for Scores")]
    public Sprite sprite20;
    public Sprite sprite40;
    public Sprite sprite60;
    public Sprite sprite80;
    public Sprite sprite100;
    public Sprite sprite500;

    private Dictionary<int, Sprite> scoreSpriteMap;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        // Initialize the score-to-sprite map
        scoreSpriteMap = new Dictionary<int, Sprite>
        {
            { 20, sprite20 },
            { 40, sprite40 },
            { 60, sprite60 },
            { 80, sprite80 },
            { 100, sprite100 },
            { 500, sprite500 }
        };

        SpawnScoreItems();
        InvokeRepeating(nameof(SpawnScoreItems), spawnInterval, spawnInterval);
    }

    private void SpawnScoreItems()
    {
        for (int i = 0; i < numberOfScoreItems; i++)
        {
            Vector2 spawnPosition = GetRandomSpawnPosition();
            if (spawnPosition != Vector2.zero)
            {
                GameObject item = Instantiate(scoreDropsPrefab, spawnPosition, Quaternion.identity);
                int scoreValue = GenerateBiasedScore(spawnPosition);

                // Assign the sprite and set the score value on the prefab
                ScoreDrop scoreItem = item.GetComponent<ScoreDrop>();
                scoreItem.SetScoreValue(scoreValue);

                SpriteRenderer spriteRenderer = item.GetComponent<SpriteRenderer>();
                if (scoreSpriteMap.ContainsKey(scoreValue))
                {
                    spriteRenderer.sprite = scoreSpriteMap[scoreValue];
                }
                else
                {
                    Debug.LogWarning($"No sprite assigned for score {scoreValue}");
                }
            }
        }
    }

    private Vector2 GetRandomSpawnPosition()
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector2 randomPosition = GetRandomPositionInView();
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(randomPosition, spawnCheckRadius, obstacleLayer);

            if (hitColliders.Length == 0)
            {
                return randomPosition;
            }
        }

        return Vector2.zero;
    }

    private Vector2 GetRandomPositionInView()
    {
        Vector3 screenBounds = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, mainCamera.transform.position.z));
        float randomX = Random.Range(-screenBounds.x, screenBounds.x);
        float randomY = Random.Range(-screenBounds.y, screenBounds.y);
        return new Vector2(randomX, randomY);
    }

    private int GenerateBiasedScore(Vector2 spawnPosition)
    {
        float distanceToPlayer = Vector2.Distance(player.position, spawnPosition);
        bool highScoreFar = Random.value < 0.6f; // 60% chance for high score to spawn far

        if (highScoreFar && distanceToPlayer > 5f)
        {
            return 500; // Jackpot
        }
        else
        {
            int[] possibleScores = { 20, 40, 60, 80, 100 };
            return possibleScores[Random.Range(0, possibleScores.Length)];
        }
    }
}
