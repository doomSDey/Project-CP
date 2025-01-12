using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    [Header("Prefab and Spawn Settings")]
    public GameObject prefabToSpawn;
    public Transform spawnPoint;
    public int n = 5;
    public float x = 10f;
    public int multiplier = 3;
    public float boostedDuration = 30f;
    public int maxActivePrefabs = 20;

    [Header("Player Settings")]
    public Transform player; // Reference to the player's transform
    public float noSpawnRadius = 5f; // Radius within which no prefabs spawn if player is nearby

    private bool isBoosted = false;
    private int currentActivePrefabs = 0;
    private Queue<GameObject> prefabPool = new Queue<GameObject>();
    private float lastSpawnCheckTime = 0f;
    private float spawnCooldown = 1f;

    public event Action<GameObject> OnDestroyed;

    private void Start()
    {
        InitializePool();
        StartCoroutine(SpawnRoutine());
        StartCoroutine(BoostRoutine());
    }

    private void InitializePool()
    {
        for (int i = 0; i < maxActivePrefabs; i++)
        {
            GameObject obj = Instantiate(prefabToSpawn);
            obj.SetActive(false);
            prefabPool.Enqueue(obj);

            // Subscribe to the OnDestroyed event if the prefab has a BaseEnemy component
            BaseEnemy baseEnemy = obj.GetComponent<BaseEnemy>();
            if (baseEnemy != null)
            {
                baseEnemy.OnDestroyed += HandlePrefabDestroyed;
            }
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (Time.time - lastSpawnCheckTime >= spawnCooldown)
            {
                int spawnCount = isBoosted ? n * multiplier : n;

                for (int i = 0; i < spawnCount; i++)
                {
                    if (currentActivePrefabs < maxActivePrefabs)
                    {
                        SpawnFromPool();
                    }
                    else
                    {
                        Debug.Log("Spawn limit reached. Waiting for prefabs to be destroyed.");
                        break;
                    }
                }

                lastSpawnCheckTime = Time.time;
            }

            yield return new WaitForSeconds(0.5f); // Reduced frequency for smoother frame rate
        }
    }

    private IEnumerator BoostRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(x);

            isBoosted = true;
            yield return new WaitForSeconds(boostedDuration);

            isBoosted = false;
        }
    }

    private void SpawnFromPool()
    {
        if (prefabPool.Count > 0)
        {
            // Check distance between player and spawner
            float distanceToPlayer = Vector3.Distance(player.position, spawnPoint.position);
            if (distanceToPlayer <= noSpawnRadius)
            {
                Debug.Log($"Skipping spawn as player is within {noSpawnRadius} units of the spawner.");
                return;
            }

            GameObject obj = prefabPool.Dequeue();

            if (!obj.activeSelf)
            {
                obj.transform.position = spawnPoint.position;
                obj.transform.rotation = Quaternion.identity;
                obj.SetActive(true);

                currentActivePrefabs++;
            }
            else
            {
                prefabPool.Enqueue(obj); // If the prefab is still active, return it to the pool
                Debug.LogWarning("Attempted to spawn an already active prefab. Skipping.");
            }
        }
    }

    public void HandlePrefabDestroyed(GameObject destroyedPrefab)
    {
        if (destroyedPrefab != null)
        {
            destroyedPrefab.SetActive(false); // Disable prefab
            prefabPool.Enqueue(destroyedPrefab); // Return to pool

            currentActivePrefabs = Mathf.Max(0, currentActivePrefabs - 1); // Decrease active prefab count safely
        }
    }
}
