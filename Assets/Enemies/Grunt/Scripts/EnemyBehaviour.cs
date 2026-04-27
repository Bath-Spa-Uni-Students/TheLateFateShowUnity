using System.Collections;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.AI;
using FMOD.Studio;
using FMODUnity;

[RequireComponent(typeof(StudioEventEmitter))]
public class EnemyBehaviour : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private EnemyStats stats;                   // Stats container (speed, damage, stoppingDistance, etc.)
    [SerializeField] private EnemyAttackMelee attackScript;      // Melee or Ranged attack script

    [Header("Player Info")]
    private Transform player;                                    // Reference to player

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 3.5f;       // Radius for detecting player
    [SerializeField] private LayerMask wallLayer;                // Layer mask for obstacles/walls
    [SerializeField] private GameObject detectionCircle;         // Optional visual for detection range

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 5f;
    [SerializeField] private float waypointArrivalDistance = 0.35f;// Distance considered "arrived" at waypoint
    [SerializeField] private float waitTimeAtWaypoint = 0.25f;

    [Header("Pack/Follower Info")]
    public bool isLeader = false;                                  // Is this enemy the pack leader?
    public Transform leader;                                       // Leader to follow
    [HideInInspector] public Vector2 followOffset;                 // Orbit offset relative to leader
    [SerializeField] private float followDistance = 1.5f;          // Orbit distance from leader
    [SerializeField] private float orbitSpeed = 2f;                // Orbit rotation speed
    [SerializeField] private float orbitStopDistance = 0.15f;      // Stop orbiting if within this distance
    [HideInInspector] public bool playerDetected = false;          // Updated detection flag
    private bool leaderDead = false;                               // Tracks if leader is dead

    [Header("Components / Internals")]
    private Rigidbody2D rb;                                        // Cached Rigidbody2D
    private BoxCollider2D boxCollider;                             // Cached BoxCollider2D
    private RigidbodyConstraints2D initialConstraints;            // Stored Rigidbody constraints
    private GameObject moveSpot;                                   // Debug move spot instance                            

    private Animator animator;

    //Patrol State
    private Vector3 spawnPosition;
    private Vector3 currentWaypoint;   // Current waypoint target
    private bool hasWaypoint = false; // Waypoint validity
    private float waitTimer = 0f;
    private bool isWaiting = false;      // Waiting at a waypoint
    private bool hasScatterTarget = false;
    private Vector3 scatterWaypoint;

    // Leader state
    [HideInInspector] public int followerCount = 0;


    // --- Enemy States (for modular state logic) ---
    private enum EnemyState
    {
        Patrol,Chase,Orbit,Scatter
    }
    private EnemyState currentState;                               // Current enemy state

    // Animator reference
    [Header("Pathtracing")]
    private Vector3 playerTarget;                                  // Target for pathfinding
    private Vector3 randomTarget;                                  // Optional random target
    private NavMeshAgent agent;                                    // NavMeshAgent reference

    // Audio
    private StudioEventEmitter emitter;
    private EventInstance gruntAttack;
    private EventInstance gruntAlert;
    private EventInstance gruntDeath;


    private void Awake()
    {
        if (stats == null)
            stats = GetComponent<EnemyStats>(); // auto-link if on same GameObject
    }
    private void Start()
    {
        InitialSetup();

        // Register the emitter with AudioManager so it is cleaned up on scene change
        emitter = AudioManager.Instance.CreateEventEmitter(FMODEvents.Instance.gruntFootsteps, this.gameObject);
        // Create attack sound instance played as a one-shot via Animation Event
        gruntAttack = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.gruntAttack);
        // Create alert sound instance played each time the enemy enters Chase state
        gruntAlert = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.gruntAlert);
        // Create death sound instance played on enemy death
        gruntDeath = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.gruntDeath);
    }

    private void InitialSetup()
    {

        // Component setup
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        attackScript = GetComponent<EnemyAttackMelee>();
        animator = GetComponent<Animator>();

        // Player must exist for detection/chasing
        var playerTag = GameObject.FindGameObjectWithTag("Player");
        // If no player found, we can still do patrol/leader-following but not detection/chasing
        player = playerTag != null ? playerTag.transform : null;


        // Store spawn position for patrol radius
        spawnPosition = transform.position;

        //NavMeshA setup
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = stats.speed;
        agent.acceleration = 140f;
        agent.stoppingDistance = stats.stoppingDistance;

        // Followers register with leader and get an orbit offset
        if (!isLeader && leader != null)
        {
            followOffset = Random.insideUnitCircle * followDistance;

            if (leader.TryGetComponent(out EnemyBehaviour leaderBehaviour))
                leaderBehaviour.RegisterFollower();
        }
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

        UpdateAnimation();
    }

    private EnemyState GetState()
    {
        // State priority:
        if (!isLeader && leaderDead)
            return EnemyState.Scatter;
        //Allows grunts to stop scattering 
        if (currentState == EnemyState.Scatter)
        {
            hasScatterTarget = false;
        }
            

        if (playerDetected && player != null)
        {
            if(currentState != EnemyState.Chase)
            {
                // Plays the alert sound when in chase state
                gruntAlert.start();
            }
            return EnemyState.Chase;
        }

        // Only orbit if we actually have a living leader assigned
        if (!isLeader && leader != null && !leaderDead)
            return EnemyState.Orbit;

        // Both leaders and followers can patrol
        return EnemyState.Patrol;
    }

    // Called by each follower on Start to let the leader know it exists
    public void RegisterFollower()
    {
        followerCount++;
        Debug.Log(gameObject.name + " follower registered. Total: " + followerCount);
    }

    // Called by each follower on death so leader can track remaining pack size
    public void UnregisterFollower()
    {
        followerCount = Mathf.Max(0, followerCount - 1);
        Debug.Log(gameObject.name + " follower died. Remaining: " + followerCount);
    }


    #region Movement States
    //simplify patrol state remove stuck detection as nav mesh will handle pathfinding around obstacles
    //three step process
    //step 1 wait at waypoint
    //step 2 pick new waypoint
    //3 check if arrived at waypoint
    private void Patrol()
    {
        // Step 1  currently waiting at a waypoint
        if (isWaiting)
        {
            waitTimer -= Time.fixedDeltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                hasWaypoint = false;
            }
            return;
        }

        // Step 2 - need a new waypoint
        if (!hasWaypoint)
        {
            if (TryGetNavMeshWaypoint(out Vector3 waypoint))
            {
                currentWaypoint = waypoint;
                hasWaypoint = true;
                agent.SetDestination(currentWaypoint);
            }
            return;
        }

        // Step 3 - check if we arrived
        float dist = Vector3.Distance(transform.position, currentWaypoint);
        if (dist <= waypointArrivalDistance)
        {
            isWaiting = true;
            waitTimer = waitTimeAtWaypoint;
        }
    }

    private bool TryGetNavMeshWaypoint(out Vector3 waypoint)
    {
        waypoint = Vector3.zero;

        for (int i = 0; i < 10; i++)
        {
            //find the nearest point on the navmesh thats valid for the agent
            Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
            Vector3 randomPoint = spawnPosition + new Vector3(randomCircle.x, randomCircle.y, 0f);
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            {
                waypoint = hit.position;
                return true;
            }
        }
        return false;
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
            agent.velocity = Vector2.zero;
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
            agent.velocity = Vector2.zero;
            return;
        }

        MoveToTarget(targetPos);
    }

    private void Scatter()
    {
        //Check if arrived at current scatter target 
        //this should make the scatter more intentional and stop the spinning
        if (hasScatterTarget)
        {
            float dist = Vector3.Distance(transform.position, scatterWaypoint);

            // Once their pick a new point
            if (dist <= waypointArrivalDistance)
                hasScatterTarget = false;

            return;
        }

        // Assign new scatter target
        if (TryGetNavMeshWaypoint(out Vector3 waypoint))
        {
            scatterWaypoint = waypoint;
            hasScatterTarget = true;
            agent.SetDestination(scatterWaypoint);
        }
    }
    #endregion

    private void UpdateDetection()
    {
        if (isLeader)
        {
            playerDetected = player != null && Vector2.Distance(rb.position, player.position) <= detectionRadius;
        }
        else
        {
            // If follower has a living leader
            if (leader != null && !leaderDead && leader.TryGetComponent(out EnemyBehaviour lb))
            {
                playerDetected = lb.playerDetected;
            }
            // If no leader detect on its own
            else
            {
                playerDetected = player != null && Vector2.Distance(rb.position, player.position) <= detectionRadius;
            }
        }
    }

    public void LeaderDied()
    {
        leaderDead = true;
        leader = null;
    }

    private void OnDestroy()
    {
        if (!isLeader)
        {

            Collider2D[] grunts = Physics2D.OverlapCircleAll(transform.position, 10f);
            foreach (Collider2D grunt in grunts)
            {
                if (grunt == null) continue;
                if (grunt.TryGetComponent(out EnemyBehaviour enemy) && !enemy.isLeader && enemy.leader == transform)
                    enemy.LeaderDied();
            }
        }
        else
        {
            // If was a follower unregister from leader so it can act independently
            if (leader != null && leader.TryGetComponent(out EnemyBehaviour leaderBehaviour))
                leaderBehaviour.UnregisterFollower();
        }
    }

    public void UpdateAnimation()
    {
        Vector2 velocity = agent.velocity;

        float speed = velocity.magnitude;

        animator.SetFloat("Speed", speed);

        if (speed > 0.01f)
        {

            Vector2 dir = velocity.normalized;

            animator.SetFloat("PosX", dir.x);
            animator.SetFloat("PosY", dir.y);
            animator.SetBool("IsWalking?", true);
        }
        else
        {
            animator.SetBool("IsWalking?", false);
        }
        UpdateSound();
    }

    // Starts or stops the spatialised footstep emitter based on the current walk state
    private void UpdateSound()
    {
        if (animator.GetBool("IsWalking?"))
        {
            // Only call Play if not already playing - avoids restarting mid-loop
            if (!emitter.IsPlaying())
                emitter.Play();
        }
        else
        {
            if (emitter.IsPlaying())
                emitter.Stop();
        }
    }

    // Called by an Animation Event on the attack frame to play the grunt attack sound
    public void PlayAttackSound()
    {
        gruntAttack.start();
    }
    void MoveToTarget(Vector3 target)
    {
        // Pathtracing movement
        agent.SetDestination(new Vector3(target.x, target.y, transform.position.z));
    }
 
}