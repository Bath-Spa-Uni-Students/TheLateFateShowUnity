using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("Player Info")]
    private Transform player;
    [SerializeField] private GameObject moveSpotGameObject; // optional debug viz (can be null)
    [SerializeField] private float startWaitTime = 0.25f;
    private float waitTimer;

    // Other Scripts
    private EnemyAttack attackScript;

    [Header("Stats")]
    [SerializeField] private float speed;
    [SerializeField] private float stoppingDistance;

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 3.5f;
    [SerializeField] private GameObject detectionCircle; // optional
    [SerializeField] private LayerMask wallLayer;

    [Header("Waypoints")]
    [SerializeField] private float waypointArrivalDistance = 0.35f; // IMPORTANT for box colliders
    [SerializeField] private int waypointMaxTries = 40;
    [SerializeField] private float waypointInflation = 0.05f; // inflates BoxCast/OverlapBox a bit

    // If we don't make progress for this long, repick waypoint
    [SerializeField] private float stuckDuration = 1.2f;
    [SerializeField] private float stuckEpsilon = 0.03f;

    [Header("Pack")]
    public bool isLeader = false;
    public Transform leader;
    [HideInInspector] public Vector2 followOffset;
    [SerializeField] private float followDistance = 1.5f;
    [SerializeField] private float orbitSpeed = 2f;
    [SerializeField] private float orbitStopDistance = 0.15f;
    [HideInInspector] public bool playerDetected = false;
    private bool leaderDead = false;

    [Header("Bounds for Patrol Waypoints")]
    [SerializeField] private GameObject gruntArea;
    [SerializeField] private Transform minX;
    [SerializeField] private Transform maxX;
    [SerializeField] private Transform minY;
    [SerializeField] private Transform maxY;

    // --- Internals ---
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private RigidbodyConstraints2D initialConstraints;

    private GameObject moveSpot; // optional debug viz
    private Vector2 currentWaypoint;
    private bool hasWaypoint;

    private float lastDistToWaypoint = Mathf.Infinity;
    private float stuckTimer = 0f;

    /// --- Pathtracing ---
    [Header("Pathtracing")]
    private Vector3 playerTarget;
    private Vector3 randomTarget;
    private NavMeshAgent agent;


    // Enemy states (for debugging and potential future expansion, currently we just switch states based on conditions in FixedUpdate)
    private enum EnemyState
    {
        Patrol,
        Chase,
        Orbit,
        Scatter
    }

    private EnemyState currentState;

   // ------------------------------------------ //

    private void Start()
    {
        InitialSetup();
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
        attackScript = GetComponent<EnemyAttack>();

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
        agent.speed = speed;
        agent.acceleration = 140f;
        agent.stoppingDistance = stoppingDistance;

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
        if (distance <= stoppingDistance)
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
        rb.linearVelocity = dir * speed;
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