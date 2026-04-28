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
                Patrol();
                break;

            case SpeedsterState.Stalk:
                StalkPlayer();
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

    private void InitialSetup()
    {
        damage = stats.damage;

        // Component setup
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        attackScript = GetComponent<SpeedsterAttack>();
        animator = GetComponent<Animator>();

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
        agent.enabled = false;

        #endregion

        waitTimer = startWaitTime;

        if (moveSpotGameObject != null)
            moveSpot = Instantiate(moveSpotGameObject, transform.position, Quaternion.identity);

        if (boxCollider != null && rb != null)
                PickNewWaypoint();
    }

    private EnemyState GetState()
    {
        // State priority:

        if (playerDetected && player != null)
        {
            if(currentState != EnemyState.Chase)
            {
                // Plays the alert sound when in chase state
                gruntAlert.start();
            }
            return EnemyState.Chase;
        }

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

   
    private void Dash(Vector2 target)
    {
        dashCooldownTimer -= Time.fixedDeltaTime;
        retargetTimer -= Time.fixedDeltaTime;

        // If currently dashing
        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;

            rb.linearVelocity = dashDirection * dashSpeed;

            if (dashTimer <= 0f)
            {
                isDashing = false;
                rb.linearVelocity = Vector2.zero;
            }

            return;
        }

        // Prevent jitter when too close
        float distance = Vector2.Distance(rb.position, target);
        if (distance < minDashDistance)
            return;

        // Try to start a new dash
        if (dashCooldownTimer <= 0f && retargetTimer <= 0f)
        {
            Vector2 toPlayer = (target - rb.position).normalized;

            // --- Overshoot ---
            float overshootDistance = 2.0f;

            // --- Side variation ---
            float sideOffset = Random.Range(-1.5f, 1.5f);
            Vector2 perpendicular = new Vector2(-toPlayer.y, toPlayer.x);

            Vector2 dashTarget = (Vector2)target
                               + toPlayer * overshootDistance
                               + perpendicular * sideOffset;

            Vector2 dir = (dashTarget - rb.position).normalized;

            // --- Prevent perfect 180 flips ---
            if (Vector2.Dot(dir, lastDashDirection) < -0.8f)
            {
                dir = Quaternion.Euler(0f, 0f, Random.Range(-45f, 45f)) * dir;
            }

            dashDirection = dir;
            lastDashDirection = dir;

            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            retargetTimer = retargetDelay;
        }
    }

    #endregion

    private void UpdateDetection()
    {
        if (player != null)
            playerDetected = Vector2.Distance(rb.position, player.position) <= detectionRadius;
        else
            playerDetected = false;
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

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (((1 << col.gameObject.layer) & wallLayer) != 0)
        {
            isDashing = false;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void Lurk()
    {
        //slow patrol using the nav mesh similar to the grunt
    }


    private void Stalk()
    {
        //robit the player at a certain distance, circling around them for a few seconds before attacking
    }

    private void Windup()
    {
        //pause for a moment, woudl like to add a windup animation if possible
    }

    private void Retreat()
    {
        //back off for a few seconds after attacking, then return to stalking or patrolling depending on if the player is still detected
    }


    private bool TryGetNavMeshWaypoint(out Vector3 waypoint)
    {

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