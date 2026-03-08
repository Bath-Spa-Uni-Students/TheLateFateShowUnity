using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
public class EnemyBehaviour : MonoBehaviour
{
    // Player info
    private Transform player;
    [SerializeField] public GameObject moveSpot;
    private float waitTime;
    [SerializeField] private float startWaitTime;

    // Enemy stats
    [SerializeField] private float speed;
    [SerializeField] private float stoppingDistance;
    [SerializeField] private float damage;

    // Detection
    [SerializeField] private float detectionRadius;
    [SerializeField] private GameObject detectionCircle;
    [SerializeField] private float moveSpotCheckRadius = 0.2f;

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

    [SerializeField] private GameObject gruntArea;
    private int childCount;

    private Rigidbody2D rb;
    private bool patroling;
    private float patrolTime;
    public bool overlappingCollider;

    [SerializeField] private Transform minX;
    private float pMinX;
    [SerializeField] private Transform maxX;
    private float pMaxX;
    [SerializeField] private Transform minY;
    private float pMinY;
    [SerializeField] private Transform maxY;
    private float pMaxY;

    void Start()
    {
        //Instantiate(moveSpot, transform);
        SetMoveSpot();
        waitTime = startWaitTime;
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
            Patrol();
            return;
        }
        ChasePlayer();
    }

    #region Movement
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

    private void Patrol()
    {
        //Debug.Log("Patrol");
        //Moves to the random spot (delta time is used so it is not frames based
        transform.position = Vector2.MoveTowards(transform.position, moveSpot.transform.position, speed * Time.deltaTime);

        //Checks if close to the spot - This is done to prevent exact checks
        if (Vector2.Distance(transform.position, moveSpot.transform.position) < 0.2f)
        {
            //Timer to make enemy wait before moving to new spot
            if (waitTime <= 0)
            {
                //sets random spot and resets the timer
                SetMoveSpot();
                waitTime = startWaitTime;
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
        }
    }
    #endregion

    #region Attack
    private void HitPlayer()
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
    #endregion

    public void SetMoveSpot()
    {
        Vector2 randomPosition;
        bool positionValid = false;

        // Keep searching until a valid position is found
        while (!positionValid)
        {
            // Generate random position inside patrol bounds
            randomPosition = new Vector2(
                UnityEngine.Random.Range(minX.position.x, maxX.position.x),
                UnityEngine.Random.Range(minY.position.y, maxY.position.y)
            );

            // Check if this position overlaps a wall
            Collider2D hit = Physics2D.OverlapCircle(randomPosition, moveSpotCheckRadius, wallLayer);

            if (hit == null)
            {
                // No wall found, position is safe
                moveSpot.transform.position = randomPosition;
                positionValid = true;
            }
        }
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