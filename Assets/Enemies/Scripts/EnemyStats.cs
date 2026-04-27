using UnityEngine;

[DisallowMultipleComponent]
public class EnemyStats : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Movement speed of the enemy.")]
    public float speed = 2.5f;

    [Tooltip("Distance to stop when chasing the player.")]
    public float stoppingDistance = 0.35f;

    [Tooltip("Radius in which the enemy detects the player.")]
    public float detectionRadius = 3.5f;

    [Header("Leader")]
    [Tooltip("Damage reduction for leader while followers alive (0-1, e.g. 0.5 = 50% reduction)")]
    public float leaderDamageReduction = 0.5f;
    private int followerCount = 0;

    [Header("Combat")]
    [Tooltip("Enemies Health")]
    public float health;
    [HideInInspector] public float maxHealth;
    [Tooltip("Damage dealt per attack.")]
    public float damage = 10f;

    [Tooltip("Time between attack attempts.")]
    public float fireRate = 0.15f;

    [Tooltip("Cooldown after an attack.")]
    public float fireCooldown = 0.5f;

    [Tooltip("Distance to player considered “close enough” for attacking.")]
    [SerializeField] public float attackCloseness;              // Distance to player considered “close enough” for attacking

    public bool canDamage = false;                              // Whether the boss can currently damage the player
    public bool canAttack = true;
    public bool isAttacking = false;


    [Header("Boss")]
    [Tooltip("Radius of the boss's shockwave attack.")]
    public float slamWaitTimer = 2f;

    [Tooltip("Spin Speed Of The Beam")]
    public float beamSpinSpeed = 100f; // Speed at which the beams spin during the attack

    [Tooltip("Duration of the beam spin attack.")]
    public float beamSpinDuration = 3f; // Duration for which the beams will spin during the attack

    [Header("Pack / AI")]
    [Tooltip("Distance to keep from the leader.")]
    public float followDistance = 1.5f;

    [Tooltip("Speed of orbiting the leader.")]
    public float orbitSpeed = 2f;



    // Get current level of player
    void Start()
    {
        // Scales health based on player level
        int playerLevel = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().currentLevel; ;
        ScaleHealth(playerLevel);
    }

    // Scales health linearly and exponentially
    public void ScaleHealth(int playerLevel)
    {
        float baseHealth = maxHealth;
        int linear = playerLevel * 3;
        int exponential = Mathf.FloorToInt(Mathf.Pow(1.05f, playerLevel));

        maxHealth = (baseHealth + linear) * exponential;
        health = maxHealth;
    }

    private void Awake()
    {
        maxHealth = health; // Set maxHealth to the initial health value
    }

    // Call this on the leader from each follower's InitialSetup
    public void RegisterFollower()
    {
        followerCount++;
    }

    public void UnregisterFollower()
    {
        followerCount--;
        // Clamp just in case
        followerCount = Mathf.Max(0, followerCount);
    }
}
