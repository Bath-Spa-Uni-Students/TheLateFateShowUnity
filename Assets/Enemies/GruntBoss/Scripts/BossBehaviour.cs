using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private EnemyStats stats;                   // Stats container (speed, damage, stoppingDistance, etc.)
    [SerializeField] private EnemyAttackMelee attackScript;      // Melee or Ranged attack script

    [Header("Player Info")]
    private Transform player;                                    // Reference to player
    [SerializeField] private GameObject moveSpotGameObject;      // Optional debug waypoint visualizer
    [SerializeField] private float startWaitTime = 0.25f;        // Wait time at waypoints
    private float waitTimer;                                     // Internal wait timer

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 3.5f;       // Radius for detecting player
    [SerializeField] private GameObject detectionCircle;         // Optional visual for detection range
    [SerializeField] private LayerMask wallLayer;                // Layer mask for obstacles/walls

    [Header("Waypoints")]
    [SerializeField] private float waypointArrivalDistance = 0.35f; // Distance considered “arrived” at waypoint
    [SerializeField] private int waypointMaxTries = 40;             // Max attempts to find a valid waypoint
    [SerializeField] private float waypointInflation = 0.05f;       // Inflates BoxCast/OverlapBox to avoid walls
    [SerializeField] private float stuckDuration = 1.2f;            // Time stuck before picking new waypoint
    [SerializeField] private float stuckEpsilon = 0.03f;            // Minimum movement to count as “progress”

    [Header("Pack/Follower Info")]
    public bool isLeader = false;                                  // Is this enemy the pack leader?
    public Transform leader;                                       // Leader to follow
    [HideInInspector] public Vector2 followOffset;                 // Orbit offset relative to leader
    [SerializeField] private float followDistance = 1.5f;          // Orbit distance from leader
    [SerializeField] private float orbitSpeed = 2f;                // Orbit rotation speed
    [SerializeField] private float orbitStopDistance = 0.15f;      // Stop orbiting if within this distance
    [HideInInspector] public bool playerDetected = false;          // Updated detection flag
    private bool leaderDead = false;                               // Tracks if leader is dead

    [Header("Grunt Area Bounds")]
    [SerializeField] private GameObject gruntArea;                // Parent object containing bounds
    [SerializeField] private Transform minX;
    [SerializeField] private Transform maxX;
    [SerializeField] private Transform minY;
    [SerializeField] private Transform maxY;

    [Header("Components / Internals")]
    private Rigidbody2D rb;                                        // Cached Rigidbody2D
    private BoxCollider2D boxCollider;                             // Cached BoxCollider2D
    private RigidbodyConstraints2D initialConstraints;            // Stored Rigidbody constraints
    private GameObject moveSpot;                                   // Debug move spot instance
    private Vector2 currentWaypoint;                               // Current waypoint target
    private bool hasWaypoint;                                      // Waypoint validity
    private float lastDistToWaypoint = Mathf.Infinity;             // Last distance to waypoint (stuck detection)
    private float stuckTimer = 0f;                                 // Stuck timer

    [Header("Pathtracing")]
    private Vector3 playerTarget;                                  // Target for pathfinding
    private Vector3 randomTarget;                                  // Optional random target
    private NavMeshAgent agent;                                    // NavMeshAgent reference

    // --- Enemy States (for modular state logic) ---
    private enum EnemyState
    {
        Patrol,
        Chase,
        Orbit,
        Scatter
    }
    private EnemyState currentState;                               // Current enemy state

    // ------------------------------------------ //

    private void Start()
    {
        InitialSetup();
    }

    private void Awake()
    {
        if (stats == null)
            stats = GetComponent<EnemyStats>(); // auto-link if on same GameObject
    }

    private void FixedUpdate()
    {
        if (rb == null) return;
        if (player == null && isLeader) return;

        UpdateDetection();

        currentState = GetState();

        // Checks each state in order of priority and executes the first one that matches (e.g. if we can chase, we chase, if not but we can orbit, we orbit, etc.)
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;

            case EnemyState.Chase:
                ChasePlayer();
                break;

            case EnemyState.Orbit:
                OrbitLeader();
                break;

            case EnemyState.Scatter:
                Scatter();
                break;
        }
    }

    private void InitialSetup()
    {

        // Component setup
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        attackScript = GetComponent<EnemyAttackMelee>();

        // Store initial constraints so we can freeze/unfreeze during attack
        initialConstraints = rb != null ? rb.constraints : RigidbodyConstraints2D.None;

        // Player must exist for detection/chasing
        var playerTag = GameObject.FindGameObjectWithTag("Player");

        // If no player found, we can still do patrol/leader-following but not detection/chasing
        player = playerTag != null ? playerTag.transform : null;

        SetGruntArea();

        #region Pathtracing Setup
        // Pathtracing setup
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        playerTarget = GameObject.FindGameObjectWithTag("Player").transform.position;

        // Pathtracing Stats
        agent.speed = stats.speed;
        agent.acceleration = 140f;
        agent.stoppingDistance = stats.stoppingDistance;

        #endregion

        waitTimer = startWaitTime;

        // Leader handles patrol waypoint selection
        if (isLeader)
        {
            if (moveSpotGameObject != null)
                moveSpot = Instantiate(moveSpotGameObject, transform.position, Quaternion.identity);

            if (boxCollider != null && rb != null)
                PickNewWaypoint();
        }

        // Followers get an initial orbit offset
        if (!isLeader && leader != null)
            followOffset = Random.insideUnitCircle * followDistance;


    }

    private EnemyState GetState()
    {
        // State priority:
        if (!isLeader && leaderDead)
            return EnemyState.Scatter;

        if (playerDetected && player != null)
            return EnemyState.Chase;

        if (!isLeader && leader != null && !leaderDead)
            return EnemyState.Orbit;

        if (isLeader)
            return EnemyState.Patrol;

        return EnemyState.Patrol;
    }

    public void TakeDamage(float damage)
    {
        // Enemy loses health
        stats.health = stats.health - damage;

        // Destroy enemy if health is less than 0
        if (stats.health <= 0)
        {
            Destroy(gameObject);
        }
    }

    #region Movement States
    private void Patrol()
    {
        // Component check - if we lost our components, just skip movement (Prevent errors)
        if (boxCollider == null || rb == null) return;

        #region Waypoint Check
        // If we don't have a waypoint, try to pick one
        if (!hasWaypoint)
        {
            PickNewWaypoint();
            return;
        }

        // Check arrival
        Vector2 pos = rb.position;
        float dist = Vector2.Distance(pos, currentWaypoint);

        // Arrived?
        if (dist <= waypointArrivalDistance)
        {
            //rb.linearVelocity = Vector2.zero;

            waitTimer -= Time.fixedDeltaTime;
            if (waitTimer <= 0f)
            {
                PickNewWaypoint();
                waitTimer = startWaitTime;
            }

            // Reset stuck tracking after arrival
            lastDistToWaypoint = Mathf.Infinity;
            stuckTimer = 0f;

            return;
        }
        #endregion

        #region Stuck Detection
        // Stuck detection (no progress)
        if (dist < lastDistToWaypoint - stuckEpsilon)
        {
            lastDistToWaypoint = dist;
            stuckTimer = 0f;
        }
        else
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= stuckDuration)
            {
                PickNewWaypoint();
                waitTimer = startWaitTime;
                lastDistToWaypoint = Mathf.Infinity;
                stuckTimer = 0f;
                return;
            }
        }
        #endregion

        MoveToTarget(currentWaypoint);
    }

    private void ChasePlayer()
    {
        // Component check - if we lost our components, just skip movement (Prevent errors)
        if (player == null) return;

        Vector2 toPlayer = (Vector2)player.position - rb.position;
        float distance = toPlayer.magnitude;

        // Check if we can hit the player from here
        if (distance <= stats.stoppingDistance)
        {
            attackScript.TryAttack();
            rb.linearVelocity = Vector2.zero;
            return;
        }

        MoveToTarget(player.position);
    }

    private void OrbitLeader()
    {
        // Component check - if we lost our components, just skip movement (Prevent errors)
        if (leader == null) return;

        // Orbit around leader
        float angle = orbitSpeed * Time.fixedDeltaTime;
        followOffset = Quaternion.Euler(0f, 0f, angle) * followOffset;

        // If we're close enough to the target orbit position, don't pathtrace (prevents jittery movement when close)
        Vector2 targetPos = (Vector2)leader.position + followOffset;
        float dist = Vector2.Distance(rb.position, targetPos);
       
        if (dist < orbitStopDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        MoveToTarget(targetPos);
    }

    private void Scatter()
    {
        // Simple scatter: keep moving in a random direction
        Vector2 dir = Random.insideUnitCircle.normalized;
        rb.linearVelocity = dir * stats.speed;
    }

    void MoveToTarget(Vector3 target)
    {
        agent.SetDestination(new Vector3(target.x, target.y, transform.position.z));
    }

    #endregion

    private void UpdateDetection()
    {
        if (isLeader)
        {
            if (player != null)
                playerDetected = Vector2.Distance(rb.position, player.position) <= detectionRadius;
            else
                playerDetected = false;
        }
        else
        {
            if (leader != null && leader.TryGetComponent(out EnemyBehaviour lb))
                playerDetected = lb.playerDetected;
            else
                playerDetected = false;
        }
    }

    // --- Waypoint picking (Leader) ---
    #region Waypoint Picking
    private void PickNewWaypoint()
    {
        if (boxCollider == null || rb == null) return;

        if (TryGetRandomWaypoint(out Vector2 waypoint))
        {
            currentWaypoint = waypoint;
            hasWaypoint = true;

            if (moveSpot != null)
                moveSpot.transform.position = waypoint;

            lastDistToWaypoint = Mathf.Infinity;
            stuckTimer = 0f;
        }
        else
        {
            hasWaypoint = false;
        }
    }

    private bool TryGetRandomWaypoint(out Vector2 waypoint)
    {
        waypoint = Vector2.zero;
        // We inflate the box collider size a bit for more forgiving waypoint picking (prevents picking waypoints that are just barely outside the collider and then getting stuck trying to get in)
        Vector2 inflatedSize = new Vector2(
            boxCollider.size.x + waypointInflation * 2f,
            boxCollider.size.y + waypointInflation * 2f
        );

        // Get the current collider center in world space, accounting for rotation
        float boxAngle = transform.eulerAngles.z;
        Vector2 currentColliderCenter = GetBoxColliderWorldCenter(boxAngle);

        // Try up to waypointMaxTries random positions within the bounds
        for (int i = 0; i < waypointMaxTries; i++)
        {
            float randomX = Random.Range(minX.position.x, maxX.position.x);
            float randomY = Random.Range(minY.position.y, maxY.position.y);

            Vector2 candidatePos = new Vector2(randomX, randomY);
            Vector2 candidateColliderCenter = candidatePos + GetRotatedOffset(boxCollider.offset, boxAngle);

            // Check overlap at the position, if it overlaps a wall it skips it immediately (prevents picking waypoints that are inside walls)
            if (Physics2D.OverlapBox(candidateColliderCenter, inflatedSize, boxAngle, wallLayer))
                continue;

            Vector2 delta = candidateColliderCenter - currentColliderCenter;
            float dist = delta.magnitude;
            if (dist < 0.01f) continue;

            Vector2 dir = delta / dist;

            if (Physics2D.BoxCast(currentColliderCenter, inflatedSize, boxAngle, dir, dist, wallLayer))
                continue;

            waypoint = candidatePos;
            return true;
        }

        return false;
    }
#endregion

    private Vector2 GetBoxColliderWorldCenter(float boxAngleDeg)
    {
        Vector2 rotatedOffset = GetRotatedOffset(boxCollider.offset, boxAngleDeg);
        return (Vector2)rb.position + rotatedOffset;
    }

    private Vector2 GetRotatedOffset(Vector2 localOffset, float boxAngleDeg)
    {
        Vector3 rotated = Quaternion.Euler(0f, 0f, boxAngleDeg) * new Vector3(localOffset.x, localOffset.y, 0f);
        return new Vector2(rotated.x, rotated.y);
    }

    public void LeaderDied()
    {
        leaderDead = true;
        leader = null;
    }

    private void OnDestroy()
    {
        if (!isLeader) return;

        Collider2D[] grunts = Physics2D.OverlapCircleAll(transform.position, 10f);
        foreach (Collider2D grunt in grunts)
        {
            if (grunt == null) continue;
            if (grunt.TryGetComponent(out EnemyBehaviour enemy) && !enemy.isLeader)
                enemy.LeaderDied();
        }
    }

    // Bounds  
    private void SetGruntArea()
    {
        if (gruntArea == null) return;

        if (minX == null) minX = gruntArea.transform.Find("minX");
        if (maxX == null) maxX = gruntArea.transform.Find("maxX");
        if (minY == null) minY = gruntArea.transform.Find("minY");
        if (maxY == null) maxY = gruntArea.transform.Find("maxY");
    }
}