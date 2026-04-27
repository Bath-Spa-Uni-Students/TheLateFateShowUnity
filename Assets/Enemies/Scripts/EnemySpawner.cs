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
    [SerializeField] private float packSpawnChance;
    [SerializeField] private float speedsterSpawnChance;

    private Coroutine spawnCoroutine;

    // Get enemy spawn time
    [SerializeField] private float gruntSpawnTime;
    [SerializeField] private float bruiserSpawnTime;
    [SerializeField] private float speedsterSpawnTime;


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
        GameObject enemyToSpawn;
        if (roll < packSpawnChance)
            enemyToSpawn = gruntPrefab; // Spawn a pack of grunts
        else if (roll < packSpawnChance + speedsterSpawnChance)
            enemyToSpawn = speedsterPrefab; // Spawn a speedster
        else
            return; // No spawn this time
        // Choose a random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        // Instantiate the enemy at the chosen spawn point
        Instantiate(enemyToSpawn, spawnPoint.position, Quaternion.identity);
    }
}
