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


    [Header("Combat")]
    [Tooltip("Enemies Health")]
    public float health = 100f;
    public float maxHealth = 100;
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


    [Header("Waypoints / Misc")]
    [Tooltip("Distance to consider waypoint reached.")]
    public float waypointArrivalDistance = 0.35f;

    [Tooltip("Duration without progress to consider stuck.")]
    public float stuckDuration = 1.2f;

    [Tooltip("Threshold to detect progress when stuck.")]
    public float stuckEpsilon = 0.03f;
}