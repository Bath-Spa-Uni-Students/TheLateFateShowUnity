using UnityEngine;
public class EnemyBehaviour : MonoBehaviour
{
    // Player info
    private Transform player;

    // Enemy stats
    [SerializeField] private float speed = 3f;
    [SerializeField] private float stoppingDistance = 0.5f;

    // Detection
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private GameObject detectionCircle;

    // Shooting
    [SerializeField] private GameObject projectile;
    [SerializeField] private float fireRate = 1f;
    private float fireTimer;

    // Wall avoidance
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float rayDistance;
    [SerializeField] private float cornerUnstickDistance;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        player = GameObject.FindGameObjectWithTag("Player").transform;

        fireTimer = fireRate;

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
        ShootPlayer();
    }

    public void ChasePlayer()
    {
        // Direction vector pointing from enemy to player
        Vector2 toPlayer = ((Vector2)player.position - rb.position);
        float distance = toPlayer.magnitude;

        // Stop if close enough to player
        if (distance <= stoppingDistance)
        {
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
                direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * cornerUnstickDistance;
            }
        }

        // Apply velocity to Rigidbody2D (physics handles collisions)
        rb.linearVelocity = direction * speed;
    }

    public void ShootPlayer()
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
    }
}