using FMOD.Studio;
using UnityEngine;
using UnityEngine.AI;

public class BossBehaviour : MonoBehaviour
{
    [Header("Boss Phases")]
    [SerializeField] private float phase2HealthThreshold = 0.5f;
    [SerializeField] private float disengageRadius = 10f;
    [SerializeField] private float sleepHealRate = 5f;

    private Vector3 spawnPosition;
    [SerializeField] private float wakeRadius = 6f;
    private bool isAwake = false;

    [Header("Stats")]
    [SerializeField] private EnemyStats stats;
    [SerializeField] private DamageHandler damageHandler;
    [SerializeField] private BossMelee meleeAttackScript;
    [SerializeField] private BossRanged rangedAttackScript;
    [SerializeField] private GameObject attackBarrier;
    [SerializeField] private GameObject damageArea;

    [Header("Player Info")]
    private Transform player;
    [SerializeField] private CapsuleCollider2D attackCapsule;
    [SerializeField] private LayerMask playerLayer;

    private ContactFilter2D filter;
    private Collider2D[] results = new Collider2D[1];

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 3.5f;
    [SerializeField] private GameObject detectionCircle;
    [HideInInspector] public bool playerDetected = false;

    [Header("Components")]
    private Rigidbody2D rb;
    private Animator animator;
    private NavMeshAgent agent;

    [Header("Teleport Barrier")]
    [SerializeField] private float teleportInFrontDistance = 1.5f;

    [Header("Combat Rhythm")]
    [SerializeField] private float advanceDuration = 3f;       // How long the boss chases before retreating after a melee
    [SerializeField] private float retreatDuration = 1.8f;     // How long the retreat phase lasts
    [SerializeField] private float retreatDistance = 3f;       // How far back the boss steps
    [SerializeField] private float warningShotHoldTime = 1.2f; // How long the boss pauses after firing a warning shot
    [SerializeField] private float warningShotCooldown = 4f;   // Minimum time between warning shots

    // Runtime rhythm tracking
    private float stateTimer = 0f;
    private float warningShotTimer = 0f;
    private Vector3 retreatTarget;

    private enum EnemyState
    {
        Sleep,
        Advance,      // Walking toward the player
        Melee,        // Close enough to swing
        Retreat,      // Stepping back to create space
        WarningShot,  // Single ranged shot fired from retreated position
        ReturnHome
    }
    private EnemyState currentState;

    // Audio
    private PARAMETER_ID phaseParamID;
    private bool musicStarted = false;
    private int currentMusicPhase = -1;
    private EventInstance bossTheme;

    public Transform Player => player;

    // Returns a world space point directly in front of the boss for the teleport barrier
    public Vector3 GetPointInFront()
    {
        return transform.position + Vector3.down * teleportInFrontDistance;
    }

    private void InitialSetup()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        meleeAttackScript = GetComponent<BossMelee>();
        rangedAttackScript = GetComponent<BossRanged>();
        attackCapsule = damageArea.GetComponent<CapsuleCollider2D>();

        detectionCircle.transform.localScale = new Vector3(detectionRadius * 2f, detectionRadius * 2f, 1f);
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        player = playerObj != null ? playerObj.transform : null;

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = stats.speed;
        agent.acceleration = 140f;
        agent.stoppingDistance = stats.stoppingDistance;
    }
    private void Awake()
    {
        if (stats == null)
            stats = GetComponent<EnemyStats>();

        filter = new ContactFilter2D();
        filter.SetLayerMask(playerLayer);
        filter.useLayerMask = true;
    }

    private void Start()
    {
        spawnPosition = transform.position;
        InitialSetup();
        bossTheme = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.bossTheme);
    }

    private void FixedUpdate()
    {
        if (rb == null || agent == null || !agent.isOnNavMesh) return;

        UpdateDetection();

        // Advance timers every tick so states know how long they have been active
        stateTimer += Time.fixedDeltaTime;
        warningShotTimer = Mathf.Max(0f, warningShotTimer - Time.fixedDeltaTime);

        EnemyState newState = GetState();

        if (newState != currentState)
        {
            // Leaving Melee or Advance disable attack barrier
            if (currentState == EnemyState.Melee || currentState == EnemyState.Advance)
                if (attackBarrier != null) attackBarrier.SetActive(false);

            currentState = newState;
            stateTimer = 0f;

            // Entering Retreat calculate the step-back destination immediately
            if (currentState == EnemyState.Retreat)
            {
                retreatTarget = transform.position + Vector3.up * retreatDistance;
                agent.SetDestination(retreatTarget);
            }

            //  fire immediately and start the cooldown
            if (currentState == EnemyState.WarningShot)
            {
                agent.ResetPath();
                rangedAttackScript.FireWarningShotSingle();
                warningShotTimer = warningShotCooldown;
            }

            if (currentState == EnemyState.Melee)
                if (attackBarrier != null) attackBarrier.SetActive(true);
        }

        switch (currentState)
        {
            case EnemyState.Sleep: Sleep(); break;
            case EnemyState.Advance: ChasePlayer(); break;
            case EnemyState.Melee: MeleeAttack(); break;
            case EnemyState.Retreat: break;
            case EnemyState.WarningShot: break; 
            case EnemyState.ReturnHome: ReturnHome(); break;
        }

        UpdateAnimation();
    }

    private EnemyState GetState()
    {
        if (player == null) return EnemyState.Sleep;

        float distance = Vector2.Distance(rb.position, player.position);

        if (distance > disengageRadius)
            return EnemyState.ReturnHome;

        // Wake up if the player enters the wake radius
        if (!isAwake)
        {
            if (distance <= wakeRadius)
            {
                animator.SetBool("Detected?", true);
                animator.ResetTrigger("Sleep");
                isAwake = true;
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.bossWake, transform.position);

                if (!musicStarted)
                {
                    bossTheme.getDescription(out EventDescription desc);
                    desc.getParameterDescriptionByName("Phase", out PARAMETER_DESCRIPTION paramDesc);
                    phaseParamID = paramDesc.id;
                    bossTheme.start();
                    musicStarted = true;
                    SetMusicPhase(0);
                }
            }
            else
            {
                return EnemyState.Sleep;
            }
        }

        // Phase 1 rhythm: Advance | Melee | Retreat | Warning shot | Advance
        if (distance <= stats.stoppingDistance)
            return EnemyState.Melee;

        if (currentState == EnemyState.Melee && stateTimer >= advanceDuration)
            return EnemyState.Retreat;

        if (currentState == EnemyState.Retreat)
        {
            bool retreatFinished = stateTimer >= retreatDuration ||
                                   Vector2.Distance(transform.position, retreatTarget) < 0.3f;
            if (retreatFinished)
                return warningShotTimer <= 0f ? EnemyState.WarningShot : EnemyState.Advance;

            return EnemyState.Retreat;
        }

        if (currentState == EnemyState.WarningShot && stateTimer >= warningShotHoldTime)
            return EnemyState.Advance;

        return currentState == EnemyState.Sleep ? EnemyState.Advance : currentState;
    }

    #region States
    private void Sleep()
    {
        animator.SetTrigger("Sleep");
        agent.ResetPath();

        if (stats.health < stats.maxHealth)
            stats.health = Mathf.Min(stats.health + sleepHealRate * Time.deltaTime, stats.maxHealth);
    }

    private void MeleeAttack()
    {
        animator.ResetTrigger("Sleep");
        if (stats.isAttacking) return;
        meleeAttackScript.TryAttack();
    }

    private void ReturnHome()
    {
        attackBarrier.SetActive(false);
        MoveToTarget(spawnPosition);

        if (Vector2.Distance(transform.position, spawnPosition) <= 2f)
        {
            isAwake = false;
            currentState = EnemyState.Sleep;
        }
    }

    private void ChasePlayer()
    {
        if (stats.isAttacking)
        {
            agent.ResetPath();
            return;
        }
        if (player == null) return;
        MoveToTarget(player.position);
    }
    #endregion

    private void MoveToTarget(Vector3 target)
    {
        if (!agent.isOnNavMesh) return;
        agent.SetDestination(new Vector3(target.x, target.y, transform.position.z));
    }

    private void UpdateDetection()
    {
        playerDetected = player != null &&
            Vector2.Distance(rb.position, player.position) <= detectionRadius;
    }

    private void UpdateAnimation()
    {
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }

    public void CheckPlayerDistance()
    {
        int hits = attackCapsule.Overlap(filter, results);
        rb.linearVelocity = Vector2.zero;
        stats.canDamage = hits > 0;
    }

    private void SetMusicPhase(int phase)
    {
        if (!musicStarted || currentMusicPhase == phase) return;
        bossTheme.setParameterByID(phaseParamID, phase);
        currentMusicPhase = phase;
    }

    public void OnBossDeath()
    {
        SetMusicPhase(2);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.bossDeath, transform.position);
    }

    public void PlayFootstepHit()
    {
        if (agent.velocity.magnitude > 0.1f)
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.bossFootsteps, transform.position);
    }   
}