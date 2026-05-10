using UnityEngine;

public class SpawnManager : MonoBehaviour
{

    public static SpawnManager Instance { get; private set; }
    //All values in script need testing
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
    private PlayerMovement playerMovement;



    //scalable variables need testing
    public int GlobalCap => baseEnemyCap + (PlayerLevel * capIncreasePerLevel); // The maximum number of enemies allowed at once, scaling with player level
    public float SpawnInterval => Mathf.Max(2f, baseSpawnInterval / (1f + PlayerLevel * spawnRateIncreasePerLevel)); // The time between spawns, decreasing as player level increases, with a minimum cap of 2 seconds
    public int MaxFollowers => maxFollowersBase + Mathf.FloorToInt(PlayerLevel / followersPerLevelThreshold); // Follower handling for grunt packs
    public int PlayerLevel => playerMovement != null ? playerMovement.currentLevel : 1; 
    public bool CanSpawn => currentEnemyCount < GlobalCap;
    public bool AllTutorialEnemiesDefeated => currentEnemyCount == 0;
    private void Awake()
    {
        if (Instance != null && Instance != this)// Ensures only one instance of SpawnManager exists
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
     private void Start()
    {

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();// Get the PlayerMovement component to access the current level for scaling
        }
    }
    public void RegisterEnemy()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerMovement = player.GetComponent<PlayerMovement>();
        currentEnemyCount++;
        Debug.Log($"Enemy registered. Total: {currentEnemyCount}/{GlobalCap}");
    }

    public void UnregisterEnemy()
    {
        currentEnemyCount = Mathf.Max(0, currentEnemyCount - 1);
        Debug.Log($"Enemy died. Total: {currentEnemyCount}/{GlobalCap}");
    }
    //called when the maze switches as all enemies will be despawned
    public void ResetEnemyCount()
    {
        currentEnemyCount = 0;
        Debug.Log("Enemy count reset.");
    }
}
