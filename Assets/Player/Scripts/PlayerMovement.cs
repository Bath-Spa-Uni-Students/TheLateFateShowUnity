using System.Collections;
using UnityEditor;
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
    [SerializeField] private float walkSpeed;
    [SerializeField] private LayerMask everythingLayer;
    [SerializeField] private LayerMask wallLayer;
    private float moveSpeed;
    public bool isInvulnerable;

    // Dash settings
    [Header("Dash Settings")]
    [SerializeField] private float dashForce;      // How fast and far the dash moves the player
    [SerializeField] private float dashTime;       // How long the dash lasts
    [SerializeField] private float dashCooldown;   // Delay before the player can dash again
    public bool hasWeapon = false;

    // Dash state checks
    bool isDashing = false; // Prevents normal movement during dash
    bool canDash = true;    // Prevents dashing again during cooldown

    private Vector2 lastMoveDir;

    [Header("References")]
    // These create variables for components in the player
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private BoxCollider2D boxCollider;
    [SerializeField] private Slider healthBar;

    //audio
    private EventInstance playerFootsteps;

    
    void Start()
    {
        healthBar.maxValue = maxHealth;
        healthBar.value = health;
        moveSpeed = walkSpeed;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        playerFootsteps = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.playerFootsteps);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Only allow normal movement if the player is NOT dashing
        if (!isDashing)
        {
            // Apply movement using Rigidbody velocity
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
            animator.SetBool("IsWalking", false);
            animator.SetFloat("LastInputX", lastMoveDir.x);
            animator.SetFloat("LastInputY", lastMoveDir.y);
        }

        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (!context.performed || !canDash || isDashing)
        return;
        //Start Dashing
    StartCoroutine(DashCoroutine());
}
    IEnumerator DashCoroutine()
    {
        isDashing = true;
        canDash = false;

        // Turn on invulnerability at the start of the dash
        isInvulnerable = true;
        //boxCollider.excludeLayers = everythingLayer; // Collide with nothing during dash
        boxCollider.includeLayers = wallLayer; // Only collide with walls during dash

        // If player hasn't moved yet, default dash direction
        if (lastMoveDir == Vector2.zero)
            lastMoveDir = Vector2.down;

        // Apply dash velocity
        rb.linearVelocity = lastMoveDir * dashForce;

        // Wait for the dash duration
        yield return new WaitForSeconds(dashTime);

        // Turn off invulnerability when dash ends
        isInvulnerable = false;

        // Stop dash
        isDashing = false;
        //boxCollider.enabled = true; // Disable collider to prevent damage during dash
        boxCollider.includeLayers = everythingLayer; // Only collide with walls during dash

        // Wait for cooldown before allowing another dash
        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    void PlayerDie()
    {
        Destroy(gameObject);
    }

    public void DamagePlayer(float damage)
    {
        if (health <= 0 || health - damage <= 0)
        {
            healthBar.value = health;
            PlayerDie();
        }
        else
        {
            health -= damage;
            healthBar.value = health;
        }
    }

    private void UpdateSound()
    {
        //start footsteps if player has an x velocity
        if(animator.GetBool("IsWalking") == true)
        {
            // get the playback state of the footsteps
            PLAYBACK_STATE playbackState;
            playerFootsteps.getPlaybackState(out playbackState);
            if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
            {
                playerFootsteps.start();
            }
        }
        // otherwise stop the footsteps
        else
        {
            playerFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }
}
