using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private float moveSpeed;

    // Dash settings
    [SerializeField] private float dashForce;      // How fast and far the dash moves the player
    [SerializeField] private float dashTime;       // How long the dash lasts
    [SerializeField] private float dashCooldown;   // Delay before the player can dash again

    // Dash state checks
    bool isDashing = false; // Prevents normal movement during dash
    bool canDash = true;    // Prevents dashing again during cooldown

    private Vector2 lastMoveDir;

    // These create variables for components in the player
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;

    void Start()
    {
        moveSpeed = playerStats.walkSpeed;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
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
        playerStats.isInvulnerable = true;

        // If player hasn't moved yet, default dash direction
        if (lastMoveDir == Vector2.zero)
            lastMoveDir = Vector2.down;

        // Apply dash velocity
        rb.linearVelocity = lastMoveDir * dashForce;

        // Wait for the dash duration
        yield return new WaitForSeconds(dashTime);

        // Turn off invulnerability when dash ends
        playerStats.isInvulnerable = false;

        // Stop dash
        isDashing = false;

        // Wait for cooldown before allowing another dash
        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }
}
