using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using FMOD.Studio;

public class PlayerMovement : MonoBehaviour
{
    [Header("Stats")]
    [Tooltip("Player Health and Walk Speed")]
    [SerializeField] public float maxHealth;
    [SerializeField] public float health;

    [Tooltip("Base movement speed of the player")]
    [SerializeField] private float walkSpeed;

    [Tooltip("Layer mask used for full collision (everything)")]
    [SerializeField] private LayerMask everythingLayer;

    [Tooltip("Layer mask used for wall-only collision during dash")]
    [SerializeField] private LayerMask wallLayer;

    // Runtime move speed (can be modified by buffs/debuffs)
    private float moveSpeed;

    // Whether the player is currently immune to damage
    public bool isInvulnerable;

    // ------------------------------------------ //

    [Header("Dash Settings")]
    [Tooltip("Force applied to the player during a dash")]
    [SerializeField] private float dashForce;

    [Tooltip("How long (in seconds) the dash lasts")]
    [SerializeField] private float dashTime;

    [Tooltip("Cooldown (in seconds) before the player can dash again")]
    [SerializeField] private float dashCooldown;

    public int fame;

    [SerializeField] private PerkSelectionUI perkSelectionUI;
    [SerializeField] private EnemyStats enemyStats;

    [SerializeField] public int currentLevel = 0;
    [SerializeField] private int currentXP = 0;


    public bool hasWeapon = true;
    bool isDashing = false;
    bool canDash = true;
    private Vector2 lastMoveDir;

    // ------------------------------------------ //

    [Header("References")]
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private BoxCollider2D boxCollider;

    [Tooltip("UI Slider that displays current player health")]
    [SerializeField] private Slider healthBar;

    // ------------------------------------------ //

    // Audio - EventInstance used here (not Emitter) because the player is always
    // at the listener position, so spatialisation is not needed
    private EventInstance playerFootsteps;
    private EventInstance playerHurt;
    private EventInstance playerDeath;
    private EventInstance playerDash;
   

    // ------------------------------------------ //

    void Start()
    {
        // Initialise health bar to match starting health value
        healthBar.maxValue = health;
        healthBar.value = health;

        // Set runtime speed to base walk speed
        moveSpeed = walkSpeed;

        // Cache components
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();

        // Create footstep audio instance via AudioManager
        playerFootsteps = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.playerFootsteps);
        playerHurt = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.playerHurt);
        playerDeath = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.playerDeath);
        playerDash = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.playerDash);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {AddFame(10); Debug.Log(currentLevel); }
    }
    void FixedUpdate()
    {
        // Only apply movement input when not mid-dash
        if (!isDashing)
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }

        UpdateSound();
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if (moveInput != Vector2.zero)
        {
            lastMoveDir = moveInput.normalized;
            animator.SetBool("IsWalking", true);
        }

        if (context.canceled)
        {
            // On input release, store last direction for idle facing and stop walk anim
            animator.SetBool("IsWalking", false);
            animator.SetFloat("LastInputX", lastMoveDir.x);
            animator.SetFloat("LastInputY", lastMoveDir.y);
        }

        // Update blend tree inputs
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (!context.performed || !canDash || isDashing)
            return;

        StartCoroutine(DashCoroutine());
    }

    IEnumerator DashCoroutine()
    {
        isDashing = true;
        canDash = false;

        // Grant invulnerability for the duration of the dash
        isInvulnerable = true;

        // Restrict collisions to walls only during the dash
        boxCollider.includeLayers = wallLayer;

        // Default dash direction if the player hasn't moved yet
        if (lastMoveDir == Vector2.zero)
            lastMoveDir = Vector2.down;

        // Apply dash velocity
        rb.linearVelocity = lastMoveDir * dashForce;
        playerDash.start();

        // Hold dash for its full duration
        yield return new WaitForSeconds(dashTime);

        // Remove invulnerability and end dash
        isInvulnerable = false;
        isDashing = false;

        // Restore full collision
        boxCollider.includeLayers = everythingLayer;

        // Wait for cooldown before allowing another dash
        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    void PlayerDie()
    {
        playerDeath.start();
        Destroy(gameObject);
    }
    public void DamagePlayer(float damage)
    {
        if (health <= 0 || health - damage <= 0)
        {
            health -= damage;
            healthBar.value = health;
            PlayerDie();
        }
        else
        {
            playerHurt.start();
            Debug.Log("Player took " + damage + " damage. Remaining health: " + (health - damage));
            health -= damage;
            healthBar.value = health;
        }
    }

    // Starts or stops the footstep audio based on the current walk state
    private void UpdateSound()
    {
        if (animator.GetBool("IsWalking"))
        {
            // Only start if not already playing
            PLAYBACK_STATE playbackState;
            playerFootsteps.getPlaybackState(out playbackState);

            if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
            {
                playerFootsteps.start();
            }
        }
        else
        {
            // Allow the tail of the sound to fade out naturally
            playerFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }

    // Add fame
    public void AddFame(int amount)
    {
        fame += amount;
        currentXP += amount;

        while (currentXP >= GetXP(currentLevel))
        {
            currentXP -= GetXP(currentLevel);
            currentLevel++;
            currentXP = 0;

            if (currentLevel == 1 || currentLevel == 3 || currentLevel == 6 || currentLevel == 9)
            {
                perkSelectionUI.Show();
            }
            else if (currentLevel == 10)
            {
                Debug.Log("Max level reached!");
                // Forced teleport goes here later
            }
        }
    }

    public int GetXP(int level)
    {
        float baseXP = 100f;
        float multiplier = 2f;
        return Mathf.FloorToInt(baseXP * Mathf.Pow(multiplier, level - 1));
    }
}