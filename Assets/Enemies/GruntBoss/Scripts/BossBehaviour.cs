using FMOD.Studio;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
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
    [SerializeField] private EnemyStats stats;                   // Stats container (speed, damage, stoppingDistance, etc.)
    [SerializeField] private DamageHandler damageHandler;        // Reference to damage handler for taking damage
    [SerializeField] private BossMelee meleeAttackScript;        // Melee or Ranged attack script
    [SerializeField] private BossRanged rangedAttackScript;      // Ranged attack script (if applicable)
    [SerializeField] private GameObject attackBarrier;           // Optional barrier that appears during attacks
    [SerializeField] private GameObject damageArea;              // Visual for the area that damages the player during melee attacks
    [SerializeField] private GameObject triBeam;

    [Header("Player Info")]
    private Transform player;                                    // Reference to player
    [SerializeField] private CapsuleCollider2D attackCapsule;    // Collider used for detecting if the player is in range during melee attacks
    [SerializeField] private LayerMask playerLayer;

    private ContactFilter2D filter;
    private Collider2D[] results = new Collider2D[1];


    [Header("Detection")]
    [SerializeField] private float detectionRadius = 3.5f;       // Radius for detecting player
    [SerializeField] private GameObject detectionCircle;         // Optional visual for detection range
    [HideInInspector] public bool playerDetected = false;        // Whether the player is currently detected (used by grunts to know when to start chasing)

    [Header("Components / Internals")]
    private Rigidbody2D rb;                                        // Cached Rigidbody2D
    private Animator animator;                                     // Cached Animator
    private NavMeshAgent agent;                                    // NavMeshAgent reference


    // --- Enemy States (for modular state logic) ---
    private enum EnemyState
    {
        Sleep,
        Chase,
        Melee,
        Ranged,
        ReturnHome
    }
    private EnemyState currentState;                               // Current enemy state

    // ------------------------------------------ //

    // Audio
    private PARAMETER_ID phaseParamID;
    private bool musicStarted = false;
    private int currentMusicPhase = -1;
    private EventInstance bossTheme;    
    private EventInstance bossWake;
    private EventInstance bossShellOpen;


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

        EnemyState newState = GetState();

        if (newState != currentState)
        {
            if (currentState == EnemyState.Ranged)
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

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = stats.speed;
        agent.acceleration = 140f;
        agent.stoppingDistance = stats.stoppingDistance;
    }

    private EnemyState GetState()
    {
        if (player == null) return EnemyState.Sleep;

        float distance = Vector2.Distance(rb.position, player.position);

        if (distance > disengageRadius)
            return EnemyState.ReturnHome;

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

        if (!phase2Active && stats.health <= stats.maxHealth * phase2HealthThreshold)
        {
            phase2Active = true;
            animator.SetBool("Phase2", true);
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.bossShellOpen, transform.position);
            if (musicStarted) SetMusicPhase(1);
        }

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
        animator.ResetTrigger("Sleep");
        if (stats.isAttacking) return;
        if (attackBarrier != null) attackBarrier.SetActive(true);
        meleeAttackScript.TryAttack();
    }

    private void RangedAttack()
    {
        animator.ResetTrigger("Sleep");
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