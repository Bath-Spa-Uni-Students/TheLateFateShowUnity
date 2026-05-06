using System.Collections;
using UnityEngine;

public class TutorialEnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject gruntPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnSolo(gruntPrefab); // Spawn a non pack grunt
    }
    private void SpawnSolo(GameObject prefab)
    {
        if (prefab == null) return;

        Transform point = GetRandomSpawnPoint();
        if (point == null) return;
        //for loop to spawn an enemy at the chosen spawn point and then deactivate that spawn point so it can't be used again
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] == point)
            {
                spawnPoints[i].gameObject.SetActive(false);
                break;
            }
        }
        GameObject enemy = Instantiate(prefab, point.position, Quaternion.identity);// Spawn the enemy at the chosen spawn point
        SpawnManager.Instance.RegisterEnemy();// Register the enemy with the SpawnManager to track the count
    }

    private Transform GetRandomSpawnPoint()//copilot wrote this for me and it looks good so I kept it, it just picks a random spawn point from the array of spawn points
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return null;
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
}
