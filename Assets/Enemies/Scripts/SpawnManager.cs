using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    //this script will handle the caps and the amount of enemies that can be spawned at a time, and will also handle the spawn points and the spawn times for each enemy type
    [Header("Global Settings")]
    [SerializeField] private int baseEnemyCap = 20;
    [SerializeField] private float baseSpawnInterval = 10f;

    [Header("Scaling")]
    [SerializeField] private int capIncreasePerLevel = 2;
    [SerializeField] private float spawnRateIncreasePerLevel = 0.1f;
    [SerializeField] private int maxFollowersBase = 1;
    [SerializeField] private int followersPerLevelThreshold = 3; // gain +1 follower every X levels

    private int currentEnemyCount = 0;
    private void Awake()
    {
        
    }
     private void Start()
    {

    }
    public void registerEnemy()
    {
        //called by the spawner script to keep track of enemies in scene
    }

    public void unregisterEnemy()
    {
        //called by the spawner script to keep track of enemies in scene
    }
    //called when the maze switches as all enemies will be despawned
    public void ResetEnemyCount()
    {
        currentEnemyCount = 0;
    }
}
