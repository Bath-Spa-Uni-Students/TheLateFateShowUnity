using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // Get enemy prefabs
    [SerializeField] private GameObject grunt;
    //[SerializeField] private GameObject bruiser;
    [SerializeField] private GameObject speedster;

    // Get enemy spawn time
    [SerializeField] private float gruntSpawnTime;
    [SerializeField] private float bruiserSpawnTime;
    [SerializeField] private float speedsterSpawnTime;

    //public GameObject grunt;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        // Start spawning enemies
        StartCoroutine(SpawnEnemy(gruntSpawnTime, grunt));
        //StartCoroutine(SpawnEnemy(bruiserSpawnTime, bruiser));
        StartCoroutine(SpawnEnemy(speedsterSpawnTime, speedster));
    }

    [SerializeField] private Transform[] spawnPoints;

    private IEnumerator SpawnEnemy(float spawnTime, GameObject enemy)
    {
        yield return new WaitForSeconds(spawnTime);
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(enemy, point.position, Quaternion.identity);
        StartCoroutine(SpawnEnemy(spawnTime, enemy));
    }
}
