using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject mucusBlobPrefab;

    [Header("Spawn Settings")]
    public int numberOfBlobs = 5;
    public float spawnInterval = 3f;        // Added: Time between spawns
    public float minimumSpawnDistance = 5f; // Added: Minimum distance from player
    public Tilemap groundTilemap;

    private Bounds tilemapBounds;
    private float spawnTimer;               // Added: Timer for spawning
    private Transform playerTransform;      // Added: Reference to player
    private Camera mainCamera;              // Added: Reference to main camera

    void Start()
    {
        if (groundTilemap != null)
        {
            tilemapBounds = groundTilemap.localBounds;
        }

        // Added: Initialize references
        mainCamera = Camera.main;
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        spawnTimer = spawnInterval;
    }

    void Update()
    {
        // Added: Spawn timer logic
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            SpawnRandomBlob();
            spawnTimer = spawnInterval;
        }
    }

    // Modified: Spawn single blob instead of multiple
    void SpawnRandomBlob()
    {
        // Get camera bounds
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        Vector3 cameraPos = mainCamera.transform.position;

        // Try to find valid spawn position
        Vector3 spawnPosition;
        int attempts = 0;
        const int maxAttempts = 30;

        do
        {
            float randomX = Random.Range(
                cameraPos.x - cameraWidth / 2,
                cameraPos.x + cameraWidth / 2
            );
            float randomY = Random.Range(
                cameraPos.y - cameraHeight / 2,
                cameraPos.y + cameraHeight / 2
            );

            spawnPosition = new Vector3(randomX, randomY, 0);
            attempts++;

            if (attempts >= maxAttempts)
            {
                Debug.LogWarning("Could not find valid spawn position");
                return;
            }
        }
        while (Vector3.Distance(spawnPosition, playerTransform.position) < minimumSpawnDistance);

        // Instantiate the Mucus Blob
        GameObject blob = Instantiate(mucusBlobPrefab, spawnPosition, Quaternion.identity);
        blob.name = "MucusBlob_" + Random.Range(1000, 9999);
    }
}
