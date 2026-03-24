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
    [Tooltip("Damage dealt per attack.")]
    public float damage = 10f;

    [Tooltip("Time between attack attempts.")]
    public float fireRate = 0.15f;

    [Tooltip("Cooldown after an attack.")]
    public float fireCooldown = 0.5f;


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