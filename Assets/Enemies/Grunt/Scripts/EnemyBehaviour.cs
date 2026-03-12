using System.Collections;
using UnityEngine;

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
    [SerializeField] private GameObject projectile;
    [SerializeField] private float fireRate;
    [SerializeField] private float fireCooldown;
    private bool canAttack = true;
    private bool isAttacking = false;

    // Wall avoidance
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float rayDistance;
    [SerializeField] private float cornerUnstickDistance;

    // Pack behaviour
    [Header("Pack")]
    public bool isLeader = false;
    public Transform leader;
    [HideInInspector] public Vector2 followOffset;
    private bool leaderDead = false;
    public float followDistance = 1.5f;
    public float orbitSpeed = 2f;
    [HideInInspector] public bool playerDetected = false;

    [SerializeField] private Transform minX;
    [SerializeField] private Transform maxX;
    [SerializeField] private Transform minY;
    [SerializeField] private Transform maxY;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        waitTime = startWaitTime;
        SetMoveSpot();

        if (!isLeader && leader != null)
            followOffset = Random.insideUnitCircle * followDistance;

        if (isLeader)
        {
            transform.localScale *= 1.25f;
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(0.7f, 0.7f, 0.7f);
        }

        if (detectionCircle != null)
            detectionCircle.transform.localScale = new Vector3(detectionRadius * 2, detectionRadius * 2, 1);
    }

    void FixedUpdate()
    {
        // Only leader checks for player
        if (isLeader)
        {
            playerDetected = Vector2.Distance(rb.position, player.position) <= detectionRadius;
        }
        else if (leader != null)
        {
            // Followers copy leader's detection
            if (leader.TryGetComponent(out EnemyBehaviour leaderBehaviour))
            {
                playerDetected = leaderBehaviour.playerDetected;
            }
        }

        // Followers scatter if leader dead
        if (!isLeader && leaderDead)
        {
            Scatter();
            return;
        }

        // Chase if player detected
        if (playerDetected)
        {
            ChasePlayer();
            return;
        }

        // Followers follow leader if not chasing
        if (!isLeader && leader != null && !leaderDead)
        {
            OrbitLeader();
            return;
        }

        if (!playerDetected)
        {
            Patrol();
            return;
        }
        // Default patrol
    }

    #region Movement
    public void ChasePlayer()
    {
        Vector2 toPlayer = (Vector2)player.position - rb.position;
        float distance = toPlayer.magnitude;

        if (distance <= stoppingDistance)
        {
            HitPlayer();
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = toPlayer.normalized;

        RaycastHit2D hit = Physics2D.Raycast(rb.position, direction, rayDistance, wallLayer);
        if (hit.collider != null)
        {
            Vector2 right = new Vector2(direction.y, -direction.x);
            Vector2 left = new Vector2(-direction.y, direction.x);

            bool rightFree = !Physics2D.Raycast(rb.position, right, rayDistance, wallLayer);
            bool leftFree = !Physics2D.Raycast(rb.position, left, rayDistance, wallLayer);

            if (rightFree && !leftFree) direction = right;
            else if (leftFree && !rightFree) direction = left;
            else if (rightFree && leftFree) direction = right;
            else direction = Vector2.zero;

            if (direction == Vector2.zero)
                direction = Random.insideUnitCircle.normalized * cornerUnstickDistance;
        }

        rb.linearVelocity = direction * speed;
    }

    private void Patrol()
    {
        MoveTowardsAvoid(moveSpot.transform.position, speed);

        if (Vector2.Distance(rb.position, moveSpot.transform.position) < 0.2f)
        {
            if (waitTime <= 0)
            {
                SetMoveSpot();
                waitTime = startWaitTime;
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
        }
    }

    public void SetMoveSpot()
    {
        bool positionValid = false;
        while (!positionValid)
        {
            Vector2 randomPos = new Vector2(
                Random.Range(minX.position.x, maxX.position.x),
                Random.Range(minY.position.y, maxY.position.y)
            );

            if (Physics2D.OverlapCircle(randomPos, moveSpotCheckRadius, wallLayer) == null)
            {
                moveSpot.transform.position = randomPos;
                positionValid = true;
            }
        }
    }

    void OrbitLeader()
    {
        float angle = orbitSpeed * Time.fixedDeltaTime;
        followOffset = Quaternion.Euler(0, 0, angle) * followOffset;

        Vector2 targetPos = (Vector2)leader.position + followOffset;
        Vector2 dir = targetPos - (Vector2)rb.position;

        if (dir.magnitude > 0.1f)
            rb.linearVelocity = dir.normalized * speed;
        else
            rb.linearVelocity = Vector2.zero;
    }

    void Scatter()
    {
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        rb.linearVelocity = randomDir * speed;
    }
    #endregion

    #region Attack
    private void HitPlayer()
    {
        if (!canAttack || isAttacking) return;
        StartCoroutine(HitCoroutine());
    }

    IEnumerator HitCoroutine()
    {
        isAttacking = true;
        canAttack = false;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezePosition;

        player.GetComponent<PlayerStats>().DamagePlayer(damage);

        yield return new WaitForSeconds(fireRate);

        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        isAttacking = false;

        yield return new WaitForSeconds(fireCooldown);
        canAttack = true;
    }
    #endregion

    public void LeaderDied()
    {
        leaderDead = true;
        leader = null;
    }

    void OnDestroy()
    {
        if (isLeader)
        {
            Collider2D[] grunts = Physics2D.OverlapCircleAll(transform.position, 10f);
            foreach (Collider2D grunt in grunts)
            {
                EnemyBehaviour enemy = grunt.GetComponent<EnemyBehaviour>();
                if (enemy != null && !enemy.isLeader)
                    enemy.LeaderDied();
            }
        }
    }

    private void MoveTowardsAvoid(Vector2 targetPosition, float moveSpeed)
    {
        Vector2 toTarget = targetPosition - rb.position;
        float distance = toTarget.magnitude;

        if (distance <= 0.1f) // Close enough, stop
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = toTarget.normalized;

        // Wall detection
        RaycastHit2D hit = Physics2D.Raycast(rb.position, direction, rayDistance, wallLayer);
        if (hit.collider != null)
        {
            // Perpendicular directions
            Vector2 right = new Vector2(direction.y, -direction.x);
            Vector2 left = new Vector2(-direction.y, direction.x);

            bool rightFree = !Physics2D.Raycast(rb.position, right, rayDistance, wallLayer);
            bool leftFree = !Physics2D.Raycast(rb.position, left, rayDistance, wallLayer);

            if (rightFree && !leftFree) direction = right;
            else if (leftFree && !rightFree) direction = left;
            else if (rightFree && leftFree) direction = right; // arbitrary if both free
            else direction = Vector2.zero;

            // If stuck, apply small random nudge
            if (direction == Vector2.zero)
                direction = Random.insideUnitCircle.normalized * cornerUnstickDistance;
        }

        rb.linearVelocity = direction * moveSpeed;
    }
}