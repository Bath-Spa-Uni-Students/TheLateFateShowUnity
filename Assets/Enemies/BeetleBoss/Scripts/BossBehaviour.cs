using FMOD.Studio;
using UnityEngine;
public class BossBehaviour : MonoBehaviour
{
    [Header("Boss Phases")]
    [SerializeField] private float phase2HealthThreshold = 0.5f;
    [SerializeField] private float disengageRadius = 10f;
    [SerializeField] private float sleepHealRate = 5f;
    [SerializeField] private float phase2SpeedMultiplier = 1.5f;

    [Header("Jitter Movement")]
    [SerializeField] private float jitterMinSpeed = 2f;
    [SerializeField] private float jitterMaxSpeed = 7f;
    [SerializeField] private float jitterMinInterval = 0.08f;
    [SerializeField] private float jitterMaxInterval = 0.28f;
    [SerializeField] private float jitterBoundRadius = 5f;

    [SerializeField] private BossLavaZone lavaZone;

    private float jitterTimer = 0f;
    private float jitterDirection = 1f;
    private float jitterSpeed = 0f;
    private Vector3 spawnPosition;
    private bool phase2Active = false;
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

    [Header("Teleport Barrier")]
    [SerializeField] private float teleportInFrontDistance = 1.5f;

    [Header("Combat Rhythm")]
    [SerializeField] private float advanceDuration = 3f;        // How long the boss chases before retreating after a melee
    [SerializeField] private float retreatDuration = 1.8f;      // How long the retreat phase lasts
    [SerializeField] private float retreatDistance = 3f;        // How far back the boss steps
    [SerializeField] private float warningShotHoldTime = 1.2f;  // How long the boss pauses after firing a warning shot
    [SerializeField] private float warningShotCooldown = 4f;    // Minimum time between warning shots in phase 1
    [SerializeField] private float p2AdvanceDuration = 2f;
    [SerializeField] private float p2RetreatDuration = 1.4f;
    [SerializeField] private float p2RetreatDistance = 2.5f;
    [SerializeField] private float p2RangedHoldTime = 2.5f;     // How long the boss fires the cone beam before advancing again

    // Runtime rhythm tracking
    private float stateTimer = 0f;
    private float warningShotTimer = 0f;
    private Vector3 retreatTarget;

    private GameManager gameManager;
    private enum EnemyState
    {
        Sleep,
        Advance,        // Walking toward the player
        Melee,          // Close enough to swing
        Retreat,        // Stepping back to create space
        WarningShot,    // Phase 1 single ranged shot fired from retreated position
        Ranged,         // Phase 2 cone beam attack
        ReturnHome
    }
    private EnemyState currentState;

    // Audio
    private PARAMETER_ID phaseParamID;
    private bool musicStarted = false;
    private int currentMusicPhase = -1;
    private EventInstance bossTheme;

    public Transform Player => player;

    // Returns a world-space point directly in front of the boss for the teleport barrier
    public Vector3 GetPointInFront()
    {
        return transform.position + Vector3.down * teleportInFrontDistance;
    }

    private void Awake()
    {
        if (stats == null)
            stats = GetComponent<EnemyStats>();

        filter = new ContactFilter2D();
        filter.SetLayerMask(playerLayer);
        filter.useLayerMask = true;
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

        rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        rangedAttackScript.enabled = false;
    }
    private void Start()
    {
        spawnPosition = transform.position;
        InitialSetup();
        bossTheme = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.bossTheme);
        PickNewJitter();
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        UpdateDetection();

        // Advance timers every tick so states know how long they have been active
        stateTimer += Time.fixedDeltaTime;
        warningShotTimer = Mathf.Max(0f, warningShotTimer - Time.fixedDeltaTime);

        EnemyState newState = GetState();

        if (newState != currentState)
        {
            // Leaving Ranged state - tell BossRanged to stop
            if (currentState == EnemyState.Ranged)
                rangedAttackScript.StopBeam();

            // Leaving Melee or Advance - disable the attack barrier
            if (currentState == EnemyState.Melee || currentState == EnemyState.Advance)
                if (attackBarrier != null) attackBarrier.SetActive(false);

            currentState = newState;
            stateTimer = 0f;

            // Entering Retreat - calculate the step-back destination immediately
            if (currentState == EnemyState.Retreat)
            {
                float dist = phase2Active ? p2RetreatDistance : retreatDistance;
                // Boss only moves on the vertical axis, so push straight up (away from player below)
                retreatTarget = transform.position + Vector3.up * dist;
            }

            // Entering WarningShot - fire immediately and start the cooldown
            if (currentState == EnemyState.WarningShot)
            {
                rangedAttackScript.FireWarningShotSingle();
                warningShotTimer = warningShotCooldown;
            }

            // Entering Melee - enable the attack barrier
            if (currentState == EnemyState.Melee)
                if (attackBarrier != null) attackBarrier.SetActive(true);
        }

        switch (currentState)
        {
            case EnemyState.Sleep: Sleep(); break;
            case EnemyState.Advance: JitterMove(); break;
            case EnemyState.Ranged: RangedAttack(); break;
        }

        UpdateAnimation();
    }

    private EnemyState GetState()
    {
        if (player == null) return EnemyState.Sleep;

        if (!isAwake)
        {
            float distance = Vector2.Distance(rb.position, player.position);
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
            else return EnemyState.Sleep;
        }

        if (!phase2Active && stats.health <= stats.maxHealth * phase2HealthThreshold)
            EnterPhase2();

        return phase2Active ? EnemyState.Ranged : EnemyState.Advance;
    }

    #region States
    private void Sleep()
    {
        animator.SetTrigger("Sleep");

        if (stats.health < stats.maxHealth)
            stats.health = Mathf.Min(stats.health + sleepHealRate * Time.deltaTime, stats.maxHealth);
    }

    private void MeleeAttack()
    {
        animator.ResetTrigger("Sleep");
        if (stats.isAttacking) return;
        meleeAttackScript.TryAttack();
    }

    private void RangedAttack()
    {
        animator.ResetTrigger("Sleep");
        rangedAttackScript.enabled = true;
        rangedAttackScript.FireConeBeams();
    }

    private void ReturnHome()
    {
        rangedAttackScript.StopBeam();
        attackBarrier.SetActive(false);
        if (Vector2.Distance(transform.position, spawnPosition) <= 2f)
        {
            isAwake = false;
            currentState = EnemyState.Sleep;
        }
    }
    #endregion

    private void UpdateDetection()
    {
        playerDetected = player != null &&
            Vector2.Distance(rb.position, player.position) <= detectionRadius;
    }

    private void UpdateAnimation()
    {
        animator.SetFloat("Speed", rb.linearVelocity.magnitude);
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
        rangedAttackScript.StopBeam();
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.bossDeath, transform.position);
        if (gameManager != null)
            gameManager.OnGameWin();
    }

    public void PlayFootstepHit()
    {
        if (rb.linearVelocity.magnitude > 0.1f)
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.bossFootsteps, transform.position);
    }

    #region Jitter (for phase 1 attacks)

    private void PickNewJitter()
    {
        jitterDirection = Random.value > 0.5f ? 1f : -1f;
        jitterSpeed = Random.Range(jitterMinSpeed, jitterMaxSpeed);
        jitterTimer = Random.Range(jitterMinInterval, jitterMaxInterval);
    }

    private void JitterMove()
    {
        jitterTimer -= Time.fixedDeltaTime;
        if (jitterTimer <= 0f) PickNewJitter();

        float xOffset = transform.position.x - spawnPosition.x;
        if (Mathf.Abs(xOffset) >= jitterBoundRadius)
            jitterDirection = -Mathf.Sign(xOffset);

        rb.linearVelocity = new Vector2(jitterDirection * jitterSpeed, 0f);
    }
    #endregion
    private void EnterPhase2()
    {
        phase2Active = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("Phase2", true);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.bossShellOpen, transform.position);
        lavaZone.gameObject.SetActive(false);
        rangedAttackScript.enabled = true;
        if (musicStarted) SetMusicPhase(1);
    }
}