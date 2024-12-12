using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject mucusBlobPrefab; // Drag & drop Mucus Blob prefab here

    [Header("Spawn Settings")]
    public int numberOfBlobs = 5; // Number of Mucus Blobs to spawn
    public Tilemap groundTilemap; // Drag & drop the Tilemap GameObject here

    private Bounds tilemapBounds;

    void Start()
    {
        // Get the bounds of the tilemap
        if (groundTilemap != null)
        {
            tilemapBounds = groundTilemap.localBounds;
        }

        // Spawn multiple blobs at random positions
        SpawnRandomBlobs(numberOfBlobs);
    }

    void SpawnRandomBlobs(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Random X and Y positions within tilemap bounds
            float randomX = Random.Range(tilemapBounds.min.x, tilemapBounds.max.x);
            float randomY = Random.Range(tilemapBounds.min.y, tilemapBounds.max.y);

            Vector3 spawnPosition = new Vector3(randomX, randomY, 0);

            // Instantiate the Mucus Blob
            GameObject blob = Instantiate(mucusBlobPrefab, spawnPosition, Quaternion.identity);
            blob.name = $"MucusBlob_{i + 1}";
        }
    }
}
