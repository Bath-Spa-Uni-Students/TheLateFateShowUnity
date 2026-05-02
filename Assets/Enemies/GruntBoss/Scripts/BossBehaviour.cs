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
    private bool phase2Active = false;
    [SerializeField] private float wakeRadius = 6f;
    private bool isAwake = false;

    [Header("Stats")]
    [SerializeField] private EnemyStats stats; // Ensure this is assigned in the Inspector or via code
    [SerializeField] private DamageHandler damageHandler;// Reference to the damage handler for health management
    [SerializeField] private BossMelee meleeAttackScript;// Reference to the melee attack script
    [SerializeField] private BossRanged rangedAttackScript;//   Reference to the ranged attack script
    [SerializeField] private GameObject attackBarrier;
    [SerializeField] private GameObject damageArea;
    [SerializeField] private GameObject triBeam;

    [Header("Player Info")]
    private Transform player;
    [SerializeField] private CapsuleCollider2D attackCapsule;// Reference to the capsule collider used for melee attack hit detection
    [SerializeField] private LayerMask playerLayer;// Layer mask to detect the player during melee attacks

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

    private enum EnemyState
    {
        Sleep,
        Chase,
        Melee,
        Ranged,
        ReturnHome
    }
    private EnemyState currentState;

    // Audio
    private PARAMETER_ID phaseParamID;
    private bool musicStarted = false;
    private int currentMusicPhase = -1;
    private EventInstance bossTheme;

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
        if (rb == null || agent == null || !agent.isOnNavMesh) return;// Safety check to prevent errors if components are missing or NavMeshAgent is not properly set up

        UpdateDetection();

        EnemyState newState = GetState();

        if (newState != currentState)
        {
            if (currentState == EnemyState.Ranged)// If we're leaving the Ranged state, make sure to disable the beam and ranged attack script
            {
                if (triBeam != null) triBeam.SetActive(false);
                rangedAttackScript.enabled = false;
            }
            currentState = newState;
        }

        switch (currentState)
        {
            case EnemyState.Sleep: Sleep(); break;
            case EnemyState.ReturnHome: ReturnHome(); break;
            case EnemyState.Chase: ChasePlayer(); break;
            case EnemyState.Melee: MeleeAttack(); break;
            case EnemyState.Ranged: RangedAttack(); break;
        }

        UpdateAnimation();
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
        // Setup NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = stats.speed;
        agent.acceleration = 140f;
        agent.stoppingDistance = stats.stoppingDistance;
    }

    private EnemyState GetState()
    {
        if (player == null) return EnemyState.Sleep;// If player is missing, default to Sleep state

        float distance = Vector2.Distance(rb.position, player.position);

        if (distance > disengageRadius)
            return EnemyState.ReturnHome;
        // Wake up if player is within wake radius, otherwise stay asleep
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
        // Transition to phase 2 if health is below threshold
        if (!phase2Active && stats.health <= stats.maxHealth * phase2HealthThreshold)
        {
            phase2Active = true;
            animator.SetBool("Phase2", true);
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.bossShellOpen, transform.position);
            if (musicStarted) SetMusicPhase(1);
        }
        // If in phase 2 and player is within extended detection range, switch to ranged attack
        if (phase2Active && distance <= detectionRadius * 1.5f)
        {
            animator.SetBool("Detected?", true);
            return EnemyState.Ranged;
        }

        if (distance <= stats.stoppingDistance)
            return EnemyState.Melee;

        return EnemyState.Chase;
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
        animator.ResetTrigger("Sleep");// Ensure we don't play the sleep animation while attacking
        if (stats.isAttacking) return;
        if (attackBarrier != null) attackBarrier.SetActive(true);
        meleeAttackScript.TryAttack();
    }

    private void RangedAttack()
    {
        animator.ResetTrigger("Sleep");// Ensure we don't play the sleep animation while attacking
        rangedAttackScript.enabled = true;
        rangedAttackScript.SpinBeam();
        MoveToTarget(player.position);
    }

    private void ReturnHome()
    {
        triBeam.SetActive(false);
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