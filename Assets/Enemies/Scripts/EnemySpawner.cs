using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject gruntPrefab;
    [SerializeField] private GameObject speedsterPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Chances")]
    [SerializeField] private float packSpawnChance = 0.4f;
    [Range(0f, 1f)]
    [SerializeField] private float speedsterSpawnChance = 1f; //set to 1 for testing, will adjust later

    private Coroutine spawnCoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnCoroutine = StartCoroutine(SpawnLoop());

    }


    private IEnumerator SpawnLoop()
    {
        while (true)
        {
           yield return new WaitForSeconds(SpawnManager.Instance.SpawnInterval);

            if (SpawnManager.Instance.CanSpawn)
                SpawnEnemy();
            // If cap is hit, loop just waits another interval and tries again
        }
    }

    public void SpawnEnemy()
    {
        // Determine which enemy to spawn based on chances
        float roll = Random.value;
       
        if (roll < speedsterSpawnChance)
        {
            SpawnSolo(speedsterPrefab);
        }
            
        else if (roll < packSpawnChance + speedsterSpawnChance)
        {
            SpawnSolo(gruntPrefab);//jsut spawn a grunt for testing
        }
        else
        {
            SpawnSolo(gruntPrefab); // Spawn a non pack grunt
        }
    }

    private void SpawnSolo(GameObject prefab)
    {
        if (prefab == null) return;

        Transform point = GetRandomSpawnPoint();
        if (point == null) return;

        GameObject enemy = Instantiate(prefab, point.position, Quaternion.identity);// Spawn the enemy at the chosen spawn point
        SpawnManager.Instance.RegisterEnemy();// Register the enemy with the SpawnManager to track the count
    }

    private Transform GetRandomSpawnPoint()//copilot wrote this for me and it looks good so I kept it, it just picks a random spawn point from the array of spawn points
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return null;
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
}
