using System.Collections;
using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    [Header("Prefab and Spawn Settings")]
    public GameObject prefabToSpawn; // Prefab to spawn
    public Transform spawnPoint; // Where to spawn the prefab
    public int n = 5; // Number of prefabs to spawn each second
    public float x = 10f; // Interval in seconds to trigger multiplier spawn
    public int multiplier = 3; // Multiplier for the number of prefabs
    public float boostedDuration = 30f; // Duration of the boosted spawn rate in seconds

    private bool isBoosted = false;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
        StartCoroutine(BoostRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            int spawnCount = isBoosted ? n * multiplier : n;

            for (int i = 0; i < spawnCount; i++)
            {
                Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(1f); // Spawn prefabs every second
        }
    }

    private IEnumerator BoostRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(x); // Wait for the interval before boosting

            isBoosted = true; // Activate boosted spawn rate
            yield return new WaitForSeconds(boostedDuration); // Wait for boosted duration

            isBoosted = false; // Return to normal spawn rate
        }
    }
}
