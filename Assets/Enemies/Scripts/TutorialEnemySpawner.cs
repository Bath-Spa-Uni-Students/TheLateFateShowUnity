using System.Collections;
using UnityEngine;

public class TutorialEnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject gruntPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints; 

    [Header("Settings")]
    [SerializeField] private int enemiesToSpawn = 3;   

    private int aliveCount = 0;
    private bool hasSpawned = false;

    public void SpawnTutorialEnemies()
    {
        if (hasSpawned) return;
        hasSpawned = true;
        aliveCount = 0;

        int spawned = 0;
        for (int i = 0; i < spawnPoints.Length && spawned < enemiesToSpawn; i++)
        {
            if (spawnPoints[i] == null) continue;

            GameObject enemy = Instantiate(gruntPrefab, spawnPoints[i].position, Quaternion.identity);

            // Hook into the grunt's death event so we know when it dies
            TutorialEnemy te = enemy.GetComponent<TutorialEnemy>();
            if (te != null)
            {
                te.OnDeath += HandleEnemyDeath;
                Debug.Log("TutorialEnemySpawner: Spawned a grunt and subscribed to its death event.");
            }
            else
                Debug.LogWarning($"TutorialEnemySpawner: Grunt prefab is missing a TutorialEnemy component on spawn point {i}.");

            SpawnManager.Instance.RegisterEnemy();
            aliveCount++;
            spawned++;
        }

        Debug.Log($"TutorialEnemySpawner: Spawned {aliveCount} tutorial enemies.");
    }

    // Death callback

    private void HandleEnemyDeath()
    {
        aliveCount = Mathf.Max(0, aliveCount - 1);
        SpawnManager.Instance.UnregisterEnemy();

        Debug.Log($"TutorialEnemySpawner: Enemy killed. Remaining: {aliveCount}");

        if (aliveCount == 0)
            TutorialManager.Instance.OnAllTutorialEnemiesDefeated();
    }

    public bool AllDefeated => hasSpawned && aliveCount == 0;
}