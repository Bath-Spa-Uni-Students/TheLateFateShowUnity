using UnityEngine;
using UnityEngine.AI;
using FMOD.Studio;
using FMODUnity;

[RequireComponent(typeof(StudioEventEmitter))]
public class SpeedsterBehaviour : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private EnemyStats stats;                   // Stats container (speed, damage, stoppingDistance, etc.)
    [SerializeField] private SpeedsterAttack attackScript;      // Melee or Ranged attack script

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 6f;       // Radius for detecting player
    [SerializeField] private float losePlayerRadius = 8f;      // Radius for losing player
    [SerializeField] private LayerMask wallLayer;                // Layer mask for obstacles/walls

    [Header("Patrol Settings")]
    [SerializeField] private float stalkRadius = 4f;             // Distance it circles the player at
    [SerializeField] private float stalkSpeed = 2.5f;            // Speed while stalking
    [SerializeField] private float stalkOrbitSpeed = 1.5f;       // How fast it circles
    [SerializeField] private float stalkDuration = 2f;           // How long it stalks before attacking
    [SerializeField] private float stalkDurationVariance = 1f;   // Random variance on stalk duration


    [Header("Windup Settings")]
    [SerializeField] private float windupDuration = 0.5f;        // How long it pauses before dashing

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 14f;
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float dashCooldown = 1.5f;          // Cooldown before it can dash again

    [Header("Retreat Settings")]
    [SerializeField] private float retreatDistance = 4f;         // How far it backs off after attacking
    [SerializeField] private float retreatSpeed = 3f;            // Speed during retreat
    [SerializeField] private float retreatArrivalDistance = 0.5f;

    // Components
    private Rigidbody2D rb;
    private Animator animator;
    private NavMeshAgent agent;
    private Transform player;

    // State machine
    private enum SpeedsterState { Lurk, Stalk, Windup, Dash, Retreat }
    private SpeedsterState currentState;

    // Detection
    private bool playerDetected = false;

    // Patrol state
    private Vector3 spawnPosition;
    private Vector3 currentWaypoint;
    private bool hasWaypoint = false;
    private bool isWaiting = false;
    private float waitTimer = 0f;

    // Stalk state
    private float stalkTimer = 0f;
    private float currentStalkDuration = 0f;
    private Vector2 stalkOrbitOffset;

    // Windup state
    private float windupTimer = 0f;

    // Dash state
    private Vector2 dashDirection;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private bool dashHitPlayer = false;

    // Retreat state
    private Vector3 retreatTarget;
    private bool hasRetreatTarget = false;

    // Audio
    private StudioEventEmitter emitter;
    private EventInstance speedsterAlert;
    private EventInstance speedsterAttack;


    private void Awake()
    {
        if (stats == null)
            stats = GetComponent<EnemyStats>(); // auto-link if on same GameObject
    }
    private void Start()
    {
        InitialSetup();

        // Placeholder audio using grunt sounds until speedster sounds are made
        emitter = AudioManager.Instance.CreateEventEmitter(FMODEvents.Instance.gruntFootsteps, this.gameObject);
        speedsterAlert = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.gruntAlert);
        speedsterAttack = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.gruntAttack);
    }

    private void InitialSetup()
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
        agent.speed = patrolSpeed;
        agent.acceleration = 140f;
        agent.stoppingDistance = 0f;

        currentState = SpeedsterState.Lurk;
    }

    private void FixedUpdate()
    {
        if (rb == null || player == null) return;

        UpdateDetection();


        currentState = GetState();

        // Checks each state in order of priority and executes the first one that matches (e.g. if we can chase, we chase, if not but we can orbit, we orbit, etc.)
        switch (currentState)
        {
            case SpeedsterState.Lurk:
                Lurk();
                break;

            case SpeedsterState.Stalk:
                Stalk();
                break;

            case SpeedsterState.Windup:
                Windup();
                break;

            case SpeedsterState.Dash:
                Dash();
                break;

            case SpeedsterState.Retreat:
                Retreat();
                break;
        }

        UpdateAnimation();
    }

   

    private SpeedsterState GetState()
    {
        if (!playerDetected)
            return SpeedsterState.Lurk;

        if (playerDetected && currentState == SpeedsterState.Lurk)
        {
            Stalk();
            return SpeedsterState.Stalk;
        }

        return currentState;
    }

    private void UpdateDetection()
    {
        if (player == null) return;

        float dist = Vector2.Distance(rb.position, player.position);

        if (!playerDetected && dist <= detectionRadius)
        {
            playerDetected = true;
            speedsterAlert.start();
        }
        else if (playerDetected && dist > losePlayerRadius)
        {
            // Lost the player - go back to lurking
            playerDetected = false;
        }
    }

    #region States

    private void Lurk()
    {
        //slow patrol using the nav mesh similar to the grunt
        agent.speed = patrolSpeed;

        if (isWaiting)
        {
            agent.velocity = Vector3.zero;// Stop moving while waiting
            waitTimer -= Time.fixedDeltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                hasWaypoint = false;
            }
            return;
        }

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

        float dist = Vector3.Distance(transform.position, currentWaypoint);
        if (dist <= waypointArrivalDistance)
        {
            isWaiting = true;
            waitTimer = waitTimeAtWaypoint;
        }
    }


    private void Stalk()
    {
        //robit the player at a certain distance, circling around them for a few seconds before attacking
        agent.speed = stalkSpeed;
        stalkTimer += Time.fixedDeltaTime;

        // Orbit around the player at stalkRadius
        float angle = stalkOrbitSpeed * Time.fixedDeltaTime;
        stalkOrbitOffset = Quaternion.Euler(0f, 0f, angle) * stalkOrbitOffset;

        Vector2 targetPos = (Vector2)player.position + stalkOrbitOffset.normalized * stalkRadius;

        agent.SetDestination(new Vector3(targetPos.x, targetPos.y, transform.position.z));

    
    }

    private void Windup()
    {
        //pause for a moment, woudl like to add a windup animation if possible
        // Stand still and face player during windup
        agent.velocity = Vector3.zero;

        windupTimer -= Time.fixedDeltaTime;

        if (windupTimer <= 0f)
            Dash();
    }
    private void Dash()
    {

        dashTimer -= Time.fixedDeltaTime;// Move in the dash direction at dash speed

        rb.linearVelocity = dashDirection * dashSpeed;

        // Check if we hit the player during the dash
        float distToPlayer = Vector2.Distance(rb.position, player.position);
        if (distToPlayer <= stats.stoppingDistance && !dashHitPlayer)// prevents multi hits
        {
            dashHitPlayer = true;
            attackScript.OnDashHit();
        }

        // Dash finished or cancelled by wall (OnCollisionEnter2D handles wall cancel)
        if (dashTimer <= 0f)
           Retreat();
    }
    private void Retreat()
    {
        //back off for a few seconds after attacking, then return to stalking or patrolling depending on if the player is still detected
        if (!hasRetreatTarget) return;

        agent.SetDestination(retreatTarget);

        float dist = Vector3.Distance(transform.position, retreatTarget);

        // Reached retreat point - go back to stalking
        if (dist <= retreatArrivalDistance)
        {
            hasRetreatTarget = false;
            Stalk();
            currentState = SpeedsterState.Stalk;
        }
    }

    #endregion

    private bool TryGetNavMeshWaypoint(out Vector3 waypoint)
    {
        //ripped straight from grunt behaviour, tries to find a random point on the nav mesh within patrol radius, returns false if it fails after several attempts
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

    private void OnCollisionEnter2D(Collision2D col)
    {
        // Cancel dash on wall hit
        if (((1 << col.gameObject.layer) & wallLayer) != 0 && currentState == SpeedsterState.Dash)
        {
            Debug.Log("Speedster dash cancelled by wall");
            Retreat();
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
            UpdateSound();
        }
        else
        {
            animator.SetBool("IsWalking?", false);
            UpdateSound();
        }
    }
    private void UpdateSound()
    {
        if (animator.GetBool("IsWalking?"))
        {
            if (!emitter.IsPlaying())
                emitter.Play();
        }
        else
        {
            if (emitter.IsPlaying())
                emitter.Stop();
        }
    }

        public void PlayAttackSound()
    {
        speedsterAttack.start();
    }
}