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

    [Header("Stats")]
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private float stoppingDistance = 0.35f;
    [SerializeField] private float damage = 10f;

    // Attack (melee via DamagePlayer)
    [SerializeField] private float fireRate = 0.15f;
    [SerializeField] private float fireCooldown = 0.5f;

    private bool canAttack = true;
    private bool isAttacking = false;

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
    NavMeshAgent agent;

    private void Start()
    {
        InitialSetup();
    }

    private void FixedUpdate()
    {
        if (rb == null) return;
        if (player == null && isLeader) return; // can't detect/chase without player

        // If not leader, follow leader's detection
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

        if (!isLeader && leaderDead)
        {
            Scatter();
            return;
        }

        if (playerDetected && player != null)
        {
            ChasePlayer();
            return;
        }

        if (!isLeader && leader != null && !leaderDead)
        {
            OrbitLeader();
            return;
        }

        // Leader patrols
        if (isLeader)
            Patrol();
    }

    private void InitialSetup()
    {
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

        // Component setup
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();

        // Store initial constraints so we can freeze/unfreeze during attack
        initialConstraints = rb != null ? rb.constraints : RigidbodyConstraints2D.None;

        // Player must exist for detection/chasing
        var playerTag = GameObject.FindGameObjectWithTag("Player");

        // If no player found, we can still do patrol/leader-following but not detection/chasing
        player = playerTag != null ? playerTag.transform : null;

        SetGruntArea();

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

        SetAgentPosition(currentWaypoint);
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
            HitPlayer();
            rb.linearVelocity = Vector2.zero;
            return;
        }

        SetAgentPosition(player.position);
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
       
        if (dist < 0.15f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        SetAgentPosition(targetPos);
    }

    private void Scatter()
    {
        // Simple scatter: keep moving in a random direction
        Vector2 dir = Random.insideUnitCircle.normalized;
        rb.linearVelocity = dir * speed;
    }
    #endregion

    // --- Waypoint picking (Leader) ---
    private void PickNewWaypoint()
    {
        // Component check - if we lost our components, just skip movement (Prevent errors)
        if (boxCollider == null || rb == null) return;

        // Inflated box size to keep clear of walls
        Vector2 inflatedSize = new Vector2(boxCollider.size.x + waypointInflation * 2f, boxCollider.size.y + waypointInflation * 2f);
        float boxAngle = transform.eulerAngles.z;
        int tries = waypointMaxTries;

        // Get current collider center in world space (for path checking)
        Vector2 currentColliderCenter = GetBoxColliderWorldCenter(boxAngle);

        #region Random Point Generation
        // Try random points until we find a valid one
        for (int i = 0; i < tries; i++)
        {
            float randomX = Random.Range(minX.position.x, maxX.position.x);
            float randomY = Random.Range(minY.position.y, maxY.position.y);

            Vector2 candidateTransformPos = new Vector2(randomX, randomY);

            Vector2 candidateColliderCenter = candidateTransformPos + GetRotatedOffset(boxCollider.offset, boxAngle);

            // 1) Spot validity: candidate box does not overlap wall
            Collider2D spotHit = Physics2D.OverlapBox(candidateColliderCenter, inflatedSize, boxAngle, wallLayer);
            if (spotHit != null) continue;

            // 2) Path validity: box cast from current collider center to candidate
            Vector2 delta = candidateColliderCenter - currentColliderCenter;
            float dist = delta.magnitude;
            if (dist < 0.01f) continue;

            Vector2 dir = delta / dist;

            RaycastHit2D pathHit = Physics2D.BoxCast(
                currentColliderCenter,
                inflatedSize,
                boxAngle,
                dir,
                dist,
                wallLayer
            );

            if (pathHit.collider != null) continue;

            // Success
            currentWaypoint = candidateTransformPos;
            hasWaypoint = true;

            if (moveSpot != null)
                moveSpot.transform.position = currentWaypoint;

            // reset patrol timers
            lastDistToWaypoint = Mathf.Infinity;
            stuckTimer = 0f;

            return;
        }

        #endregion

        // If nothing found, keep current (or just stop)
        hasWaypoint = false;
    }

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

    // --- Attack ---
    #region Attack
    private void HitPlayer()
    {
        if (!canAttack || isAttacking || player == null) return;
        StartCoroutine(HitCoroutine());
    }

    private IEnumerator HitCoroutine()
    {
        isAttacking = true;
        canAttack = false;

        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezePosition;

        var stats = player.GetComponent<PlayerMovement>();
        if (stats != null)
            stats.DamagePlayer(damage);

        yield return new WaitForSeconds(fireRate);

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        isAttacking = false;

        yield return new WaitForSeconds(fireCooldown);
        canAttack = true;
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
    #endregion

    // Bounds  
    private void SetGruntArea()
    {
        if (gruntArea == null) return;

        if (minX == null) minX = gruntArea.transform.Find("minX");
        if (maxX == null) maxX = gruntArea.transform.Find("maxX");
        if (minY == null) minY = gruntArea.transform.Find("minY");
        if (maxY == null) maxY = gruntArea.transform.Find("maxY");
    }

    void SetAgentPosition(Vector3 target)
    {
        agent.SetDestination(new Vector3(target.x, target.y, transform.position.z));
    }
}