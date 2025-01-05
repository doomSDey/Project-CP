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

    private bool isBoosted = false;
    private int currentActivePrefabs = 0;
    private Queue<GameObject> prefabPool = new Queue<GameObject>();

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

            // Subscribe to the OnDestroyed event
            obj.GetComponent<BaseEnemy>().OnDestroyed += HandlePrefabDestroyed;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
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

            yield return new WaitForSeconds(1f);
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
            GameObject obj = prefabPool.Dequeue();
            obj.transform.position = spawnPoint.position;
            obj.transform.rotation = Quaternion.identity;
            obj.SetActive(true);

            currentActivePrefabs++;
        }
    }

    private void HandlePrefabDestroyed(GameObject destroyedPrefab)
    {
        destroyedPrefab.SetActive(false);
        prefabPool.Enqueue(destroyedPrefab);

        currentActivePrefabs--;
    }
}
