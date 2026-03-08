using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
public class EnemyBehaviour : MonoBehaviour
{
    // Player info
    private Transform player;

    // Enemy stats
    [SerializeField] private float speed;
    [SerializeField] private float stoppingDistance;
    [SerializeField] private float damage;

    // Detection
    [SerializeField] private float detectionRadius;
    [SerializeField] private GameObject detectionCircle;

    // Attacking
    [SerializeField] private GameObject projectile; //Old
    [SerializeField] private float fireRate;
    [SerializeField] private float fireCooldown;
    private bool canAttack = true;
    private bool isAttacking = false;

    // Wall avoidance
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float rayDistance;
    [SerializeField] private float cornerUnstickDistance;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Setup detection circle
        if (detectionCircle != null)
            detectionCircle.transform.localScale = new Vector3(detectionRadius * 2, detectionRadius * 2, 1);
    }

    void FixedUpdate()
    {
        if (Vector2.Distance(rb.position, player.position) > detectionRadius)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        ChasePlayer();
    }

    public void ChasePlayer()
    {
        // Direction vector pointing from enemy to player
        Vector2 toPlayer = ((Vector2)player.position - rb.position);
        float distance = toPlayer.magnitude;

        // Stop if close enough to player
        if (distance <= stoppingDistance)
        {
            HitPlayer();
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = toPlayer.normalized;

        // Wall Detection
        RaycastHit2D hit = Physics2D.Raycast(rb.position, direction, rayDistance, wallLayer);

        if (hit.collider != null)
        {
            // Wall detected directly ahead, attempt to slide around
            Vector2 right = new Vector2(direction.y, -direction.x); // perpendicular right
            Vector2 left = new Vector2(-direction.y, direction.x);  // perpendicular left

            // Check if right or left is free
            bool rightFree = !Physics2D.Raycast(rb.position, right, rayDistance, wallLayer);
            bool leftFree = !Physics2D.Raycast(rb.position, left, rayDistance, wallLayer);

            // Choose direction
            if (rightFree && !leftFree)
                direction = right;
            else if (leftFree && !rightFree)
                direction = left;
            else if (rightFree && leftFree)
                direction = right; // arbitrary choice if both free
            else
                direction = Vector2.zero; // stuck

            // Corner unsticking
            // If enemy is almost not moving (stuck), push slightly forward or sideways
            if (direction == Vector2.zero)
            {
                // Try small random nudge to unstick
                direction = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized * cornerUnstickDistance;
            }
        }

        // Apply velocity to Rigidbody2D (physics handles collisions)
        rb.linearVelocity = direction * speed;
    }

    public void HitPlayer()
    {
        if (!canAttack || isAttacking)
            return;
        StartCoroutine(HitCoroutine());
    }

    IEnumerator HitCoroutine()
    {
        isAttacking = true;
        canAttack = false;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezePosition;
        player.GetComponent<PlayerStats>().DamagePlayer(damage);

        // Wait for the attack duration
        yield return new WaitForSeconds(fireRate);
        rb.constraints = RigidbodyConstraints2D.None;
        isAttacking = false;

        // Wait for cooldown before allowing another attack
        yield return new WaitForSeconds(fireCooldown);

        canAttack = true;
    }


    /*public void ShootPlayer()
    {
        if (fireTimer <= 0)
        {
            Instantiate(projectile, transform.position, Quaternion.identity);
            fireTimer = fireRate;
        }
        else
        {
            fireTimer -= Time.fixedDeltaTime;
        }
    }*/
}