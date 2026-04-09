using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.AI;

public class BossBehaviour : MonoBehaviour
{
    [Header("Boss Phases")]
    [SerializeField] private float phase2HealthThreshold = 0.5f;
    [SerializeField] private float rangedCooldown = 5f;
    [SerializeField] private float disengageRadius = 10f;
    [SerializeField] private float sleepHealRate = 5f;

    private float rangedTimer;
    private Vector3 spawnPosition;
    private bool phase2Active = false;
    private bool rangedActive = false;
    [SerializeField] private float wakeRadius = 6f;
    private bool isAwake = false;

    [Header("Stats")]
    [SerializeField] private EnemyStats stats;                   // Stats container (speed, damage, stoppingDistance, etc.)
    [SerializeField] private DamageHandler damageHandler;        // Reference to damage handler for taking damage
    [SerializeField] private BossMelee meleeAttackScript;        // Melee or Ranged attack script
    [SerializeField] private BossRanged rangedAttackScript;      // Ranged attack script (if applicable)
    [SerializeField] private GameObject attackBarrier;           // Optional barrier that appears during attacks
    [SerializeField] private GameObject damageArea;              // Visual for the area that damages the player during melee attacks
    [SerializeField] private GameObject triBeam;

    [Header("Player Info")]
    private Transform player;                                    // Reference to player
    [SerializeField] private GameObject moveSpotGameObject;      // Optional debug waypoint visualizer
    [SerializeField] private float startWaitTime = 0.25f;        // Wait time at waypoints
    private float waitTimer;                                     // Internal wait timer
    [SerializeField] private CapsuleCollider2D attackCapsule;    // Collider used for detecting if the player is in range during melee attacks
    [SerializeField] private LayerMask playerLayer;

    private ContactFilter2D filter;
    private Collider2D[] results = new Collider2D[1];


    [Header("Detection")]
    [SerializeField] private float detectionRadius = 3.5f;       // Radius for detecting player
    [SerializeField] private GameObject detectionCircle;         // Optional visual for detection range
    [SerializeField] private LayerMask wallLayer;                // Layer mask for obstacles/walls
    [HideInInspector] public bool playerDetected = false;        // Whether the player is currently detected (used by grunts to know when to start chasing)

    [Header("Waypoints")]
    [SerializeField] private float waypointArrivalDistance = 0.35f; // Distance considered “arrived” at waypoint
    [SerializeField] private int waypointMaxTries = 40;             // Max attempts to find a valid waypoint
    [SerializeField] private float waypointInflation = 0.05f;       // Inflates BoxCast/OverlapBox to avoid walls
    [SerializeField] private float stuckDuration = 1.2f;            // Time stuck before picking new waypoint
    [SerializeField] private float stuckEpsilon = 0.03f;            // Minimum movement to count as “progress”

    [Header("Boss Area Bounds")]
    [SerializeField] private GameObject gruntArea;                // Parent object containing bounds
    [SerializeField] private Transform minX;
    [SerializeField] private Transform maxX;
    [SerializeField] private Transform minY;
    [SerializeField] private Transform maxY;

    [Header("Components / Internals")]
    private Rigidbody2D rb;                                        // Cached Rigidbody2D
    private BoxCollider2D boxCollider;                             // Cached BoxCollider2D
    private Animator animator;                                     // Cached Animator
    private RigidbodyConstraints2D initialConstraints;             // Stored Rigidbody constraints
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
        Sleep,
        Patrol,
        Chase,
        Melee,
        Ranged,
        ReturnHome
    }
    private EnemyState currentState;                               // Current enemy state

    // ------------------------------------------ //

    private void Start()
    {
        spawnPosition = transform.position;
        InitialSetup();
        rangedTimer = rangedCooldown;
    }

    private void Awake()
    {
        if (stats == null)
            stats = GetComponent<EnemyStats>(); // auto-link if on same GameObject

        filter = new ContactFilter2D();
        filter.SetLayerMask(playerLayer);
        filter.useLayerMask = true;
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        UpdateDetection();

        EnemyState newState = GetState();

        if (newState != currentState)
        {
            // If we are leaving ranged attack
            if (currentState == EnemyState.Ranged)
            {
                rangedActive = false;

                if (triBeam != null)
                    triBeam.SetActive(false);

                rangedAttackScript.enabled = false;
            }

            currentState = newState;
        }

        // Checks each state in order of priority and executes the first one that matches (e.g. if we can chase, we chase, if not but we can orbit, we orbit, etc.)
        switch (currentState)
        {
            case EnemyState.Sleep:
                Sleep();
                break;

            case EnemyState.ReturnHome:
                ReturnHome();
                break;

            case EnemyState.Patrol:
                Patrol();
                break;

            case EnemyState.Chase:
                ChasePlayer();
                break;

            case EnemyState.Melee:
                MeleeAttack();
                break;

            case EnemyState.Ranged:
                RangedAttack();
                break;
        }

        UpdateAnimation();
        Debug.Log(currentState);
    }

    private void InitialSetup()
    {

        // Component setup
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        meleeAttackScript = GetComponent<BossMelee>();
        rangedAttackScript = GetComponent<BossRanged>();
        attackCapsule = damageArea.GetComponent<CapsuleCollider2D>();

        detectionCircle.transform.localScale = new Vector3(detectionRadius * 2f, detectionRadius * 2f, 1f);

        // Store initial constraints so we can freeze/unfreeze during attack
        initialConstraints = rb != null ? rb.constraints : RigidbodyConstraints2D.None;

        // Player must exist for detection/chasing
        var playerTag = GameObject.FindGameObjectWithTag("Player");

        // If no player found, we can still do patrol/leader-following but not detection/chasing
        player = playerTag != null ? playerTag.transform : null;

        SetBossArea();

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

        if (moveSpotGameObject != null)
                moveSpot = Instantiate(moveSpotGameObject, transform.position, Quaternion.identity);

        if (boxCollider != null && rb != null)
                PickNewWaypoint();
    }

    private EnemyState GetState()
    {
        if (player == null)
            return EnemyState.Sleep;

        float distance = Vector2.Distance(rb.position, player.position);

        if (distance > disengageRadius) // If the player is too far, disengage and return home
            return EnemyState.ReturnHome;

        // Wake up check
        if (!isAwake)
        {
            if (distance <= wakeRadius)
                isAwake = true;
            else
                return EnemyState.Sleep;
        }

        // Phase check
        if (!phase2Active && stats.health <= stats.maxHealth * phase2HealthThreshold)
            phase2Active = true;

        // Phase 2 prefers ranged attacks
        if (phase2Active && distance <= detectionRadius * 1.5f)
            return EnemyState.Ranged;

        // Melee check
        if (distance <= stats.stoppingDistance)
            return EnemyState.Melee;

        // Chase if player detected
        if (playerDetected)
            return EnemyState.Chase;

        return EnemyState.Sleep;
    }

    //// Phases //////
    #region Phases
    private void Sleep()
    {
        rb.linearVelocity = Vector2.zero;

        if (stats.health < stats.maxHealth)
        {
            stats.health += sleepHealRate * Time.deltaTime; // Heal over time while sleeping
            stats.health = Mathf.Min(stats.health, stats.maxHealth); // Clamp to max health
            Debug.Log("Healing in sleep. Current health: " + stats.health);
        }
    }
    private void MeleeAttack()
    {
        if (stats.isAttacking)
            return;

        if (attackBarrier != null)
            attackBarrier.SetActive(true);

        meleeAttackScript.TryAttack();
    }

    private void RangedAttack()
    {
        rangedActive = true;
        rangedAttackScript.enabled = true;
        //triBeam.SetActive(true);
        rangedAttackScript.SpinBeam();

        MoveToTarget(player.position);
    }

    private void ReturnHome()
    {
        float distance = Vector2.Distance(transform.position, spawnPosition);

        triBeam.SetActive(false); // Just in case we were in the middle of a ranged attack when we disengage
        attackBarrier.SetActive(false);

        MoveToTarget(spawnPosition);

        if (distance <= 2f)
        {
            isAwake = false;
            currentState = EnemyState.Sleep;
        }
    }
    #endregion

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
        if (stats.isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        // Component check - if we lost our components, just skip movement (Prevent errors)
        if (player == null) return;

        Vector2 toPlayer = (Vector2)player.position - rb.position;
        float distance = toPlayer.magnitude;

        // Check if we can hit the player from here
        if (distance <= stats.stoppingDistance)
        {
            meleeAttackScript.TryAttack();
            rb.linearVelocity = Vector2.zero;
            return;
        }

        MoveToTarget(player.position);
    }

    void MoveToTarget(Vector3 target)
    {
        agent.SetDestination(new Vector3(target.x, target.y, transform.position.z));
    }

    #endregion

    private void UpdateDetection()
    {
        if (player != null)
            playerDetected = Vector2.Distance(rb.position, player.position) <= detectionRadius;
        else
            playerDetected = false;
    }

    // --- Waypoint picking ---
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

    // --- Utility ---
    #region Utility
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

    private void OnDestroy()
    {

    }

    // Bounds  
    private void SetBossArea()
    {
        if (gruntArea == null) return;

        if (minX == null) minX = gruntArea.transform.Find("minX");
        if (maxX == null) maxX = gruntArea.transform.Find("maxX");
        if (minY == null) minY = gruntArea.transform.Find("minY");
        if (maxY == null) maxY = gruntArea.transform.Find("maxY");
    }
    #endregion
    private void UpdateAnimation()
    {
        Vector2 velocity = agent.velocity;

        float speed = velocity.magnitude;

        animator.SetFloat("Speed", speed);
    }

    public void CheckPlayerDistance() 
    {
        /* if (player == null) return;
        float distanceToPlayer = Vector2.Distance(rb.position, player.position);
        if (distanceToPlayer <= stats.attackCloseness)
        {
            rb.linearVelocity = Vector2.zero;
            stats.canDamage = true;
        }
        else
        {
            stats.canDamage = false;
        }*/

        int hits = attackCapsule.Overlap(filter, results);

        if (hits > 0)
        {
            rb.linearVelocity = Vector2.zero;
            stats.canDamage = true;
        }
        else
        {
            stats.canDamage = false;
        }
    }

}