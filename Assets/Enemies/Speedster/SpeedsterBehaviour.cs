using UnityEngine;
using UnityEngine.AI;
using FMOD.Studio;
using FMODUnity;

[RequireComponent(typeof(StudioEventEmitter))]
public class SpeedsterBehaviour : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private EnemyStats stats;
    [SerializeField] private SpeedsterAttack attackScript;

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 6f;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 8f;
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float waypointArrivalDistance = 0.35f;
    [SerializeField] private float waitTimeAtWaypoint = 0.5f;

    [Header("Windup Settings")]
    [SerializeField] private float windupDuration = 0.5f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 14f;
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float dashCooldown = 2f;

    [Header("Retreat Settings")]
    [SerializeField] private float retreatDistance = 4f;
    [SerializeField] private float retreatSpeed = 3f;
    [SerializeField] private float retreatArrivalDistance = 0.5f;

    // Components
    private Rigidbody2D rb;
    private Animator animator;
    private NavMeshAgent agent;
    private Transform player;

    private enum SpeedsterState { Patrol, Windup, Dash, Retreat }
    private SpeedsterState currentState;

    // Patrol state variables
    private Vector3 spawnPosition;
    private Vector3 currentWaypoint;
    private bool hasWaypoint = false;
    private bool isWaiting = false;
    private float waitTimer = 0f;

    // Windup state variables
    private float windupTimer = 0f;

    // Dash state variables
    private Vector2 dashDirection;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private bool dashHitPlayer = false;

    // Retreat state variables
    private Vector3 retreatTarget;

    // Audio
    private StudioEventEmitter emitter;
    private EventInstance speedsterAlert;
    private EventInstance speedsterAttack;

    private void Awake()
    {
        if (stats == null)
            stats = GetComponent<EnemyStats>();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        attackScript = GetComponent<SpeedsterAttack>();
        animator = GetComponent<Animator>();

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        player = playerObj != null ? playerObj.transform : null;

        spawnPosition = transform.position;

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.acceleration = 140f;
        agent.stoppingDistance = 0f;

        emitter = AudioManager.Instance.CreateEventEmitter(FMODEvents.Instance.gruntFootsteps, this.gameObject);
        speedsterAlert = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.gruntAlert);
        speedsterAttack = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.gruntAttack);

        // Always start in patrol
        EnterPatrol();
    }

    private void FixedUpdate()
    {
        if (rb == null || player == null) return;

        // Count down the dash cooldown every frame
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.fixedDeltaTime;

        // Run the current states tick method each frame
        switch (currentState)
        {
            case SpeedsterState.Patrol: TickPatrol(); break;
            case SpeedsterState.Windup: TickWindup(); break;
            case SpeedsterState.Dash: TickDash(); break;
            case SpeedsterState.Retreat: TickRetreat(); break;
        }

        UpdateAnimation();
    }

    // State Entry Methods
    // Called once when transitioning into a state, handles all setup
 

    private void EnterPatrol()
    {
        currentState = SpeedsterState.Patrol;
        hasWaypoint = false;
        isWaiting = false;
        agent.enabled = true;
        agent.isStopped = false;
        agent.speed = patrolSpeed;
    }

    private void EnterWindup()
    {
        currentState = SpeedsterState.Windup;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        speedsterAlert.start();
        animator.SetTrigger("Windup");
        // Automatically match windup duration to the animation length
        windupTimer = GetWindupAnimationLength();
    }

    private float GetWindupAnimationLength()
    {
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == "Windup")
                return clip.length;
        }
        Debug.LogWarning("Windup clip not found, defaulting to 0.5s");
        return 0.5f;
    }

    private void EnterDash()
    {
        currentState = SpeedsterState.Dash;
        // Lock in the direction to the player at the moment the dash starts
        dashDirection = ((Vector2)player.position - rb.position).normalized;
        dashTimer = dashDuration;
        dashHitPlayer = false;
        // Disable the agent so we can drive movement through the rigidbody directly
        agent.enabled = false;
        rb.linearVelocity = Vector2.zero;
    }

    private void EnterRetreat()
    {
        currentState = SpeedsterState.Retreat;
        // Kill any leftover dash momentum
        rb.linearVelocity = Vector2.zero;

        agent.enabled = true;
        agent.isStopped = false;
        agent.speed = retreatSpeed;

        // Find a point directly behind the enemy relative to the player
        Vector2 awayFromPlayer = ((Vector2)transform.position - (Vector2)player.position).normalized;
        Vector3 retreatPoint = transform.position + new Vector3(awayFromPlayer.x, awayFromPlayer.y, 0f) * retreatDistance;

        // Sample the navmesh for a valid nearby point, fall back to spawn if none found
        retreatTarget = NavMesh.SamplePosition(retreatPoint, out NavMeshHit hit, retreatDistance, NavMesh.AllAreas)
            ? hit.position
            : spawnPosition;

        dashCooldownTimer = dashCooldown;
        agent.SetDestination(retreatTarget);
    }

    // -----------------------------------------------------------------
    // State Tick Methods
    // Called every FixedUpdate while in that state
    // -----------------------------------------------------------------

    private void TickPatrol()
    {
        // Check if the player is close enough and the cooldown has expired
        float dist = Vector2.Distance(rb.position, player.position);
        if (dist <= detectionRadius && dashCooldownTimer <= 0f)
        {
            EnterWindup();
            return;
        }

        // Wait at the current waypoint before picking a new one
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

        // Pick a new random waypoint if we dont have one
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

        // Check if we have arrived at the waypoint
        if (Vector3.Distance(transform.position, currentWaypoint) <= waypointArrivalDistance)
        {
            isWaiting = true;
            waitTimer = waitTimeAtWaypoint;
        }
    }

    private void TickWindup()
    {
        // Count down the windup then launch the dash
        windupTimer -= Time.fixedDeltaTime;
        if (windupTimer <= 0f)
            EnterDash();
    }

    private void TickDash()
    {
        dashTimer -= Time.fixedDeltaTime;
        // Drive movement through the rigidbody during the dash
        rb.linearVelocity = dashDirection * dashSpeed;

        // Check if we have hit the player
        float distToPlayer = Vector2.Distance(rb.position, player.position);
        if (distToPlayer <= stats.stoppingDistance && !dashHitPlayer)
        {
            dashHitPlayer = true;
            attackScript.OnDashHit();
        }

        // End the dash when the timer runs out
        if (dashTimer <= 0f)
            EnterRetreat();
    }

    private void TickRetreat()
    {
        // Once we have arrived at the retreat point go back to patrolling
        if (Vector3.Distance(transform.position, retreatTarget) <= retreatArrivalDistance)
        {
            agent.ResetPath();
            EnterPatrol();
        }
    }

 

    private void OnCollisionEnter2D(Collision2D col)
    {
        // If we hit a wall while dashing cancel it and retreat
        if (currentState == SpeedsterState.Dash)
            EnterRetreat();
    }

    private bool TryGetNavMeshWaypoint(out Vector3 waypoint)
    {
        // Try up to 10 times to find a valid point on the navmesh within patrol radius
        waypoint = Vector3.zero;
        for (int i = 0; i < 10; i++)
        {
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

    public void UpdateAnimation()
    {
        // Use rb velocity during dash since the agent is disabled
        Vector2 velocity = agent.enabled ? (Vector2)agent.velocity : rb.linearVelocity;
        float speed = velocity.magnitude;

        animator.SetBool("IsWalking", speed > 0.01f);
        UpdateSound();
    }

    private void UpdateSound()
    {
        if (animator.GetBool("IsWalking"))
        {
            if (!emitter.IsPlaying()) emitter.Play();
        }
        else
        {
            if (emitter.IsPlaying()) emitter.Stop();
        }
    }

    public void PlayAttackSound()
    {
        speedsterAttack.start();
    }
}