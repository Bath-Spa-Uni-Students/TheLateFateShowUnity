using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // Get enemy prefabs
    [SerializeField] private GameObject grunt;
    //[SerializeField] private GameObject bruiser;
    //[SerializeField] private GameObject speedster;

    // Get enemy spawn time
    [SerializeField] private float gruntSpawnTime;
    [SerializeField] private float bruiserSpawnTime;
    [SerializeField] private float speedsterSpawnTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Start spawning enemies
        StartCoroutine(spawnEnemy(gruntSpawnTime, grunt));
        //StartCoroutine(spawnEnemy(bruiserSpawnTime, bruiser));
        //StartCoroutine(spawnEnemy(speedsterSpawnTime, speedster));
    }

    private IEnumerator spawnEnemy(float spawnTime, GameObject enemy)
    {
        yield return new WaitForSeconds(spawnTime);
        // Create new enemy in a random location
        GameObject newEnemy = Instantiate(enemy, new Vector3(Random.Range(-2f, 2), Random.Range(-3f, 3), 0), Quaternion.identity);
        StartCoroutine(spawnEnemy(spawnTime, newEnemy));
    }
}
