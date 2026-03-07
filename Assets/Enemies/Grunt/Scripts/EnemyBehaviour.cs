using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    // Info Of Player
    private Transform player;
    private ChasePlayer playerChasePlayer;
    // Enemy Stats
    [SerializeField] public float speed;
    [SerializeField] private Transform target;
    [SerializeField] private float stoppingDistance = 0.5f;

    // Enemy Weapon Stats
    [SerializeField] private float fireRate;
    [SerializeField] private GameObject projectile;
    private float fireTimer;

    // Radius around Enemy to find Player
    [SerializeField] private float detectionRadius;
    [SerializeField] private GameObject detectionCircle;

    [SerializeField] private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        detectionRadius = GetComponent<CircleCollider2D>().radius;
        detectionCircle.transform.localScale = new Vector3(detectionRadius * 2, detectionRadius * 2, detectionRadius * 2);
        // Sets target as player by making sure the target has player tag
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        fireTimer = fireRate;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        #region Deprecated
        
        //Checks if player is in detection radius
        if (Vector2.Distance(transform.position, target.position) < detectionRadius)
        {
            ChasePlayer();
            ShootPlayer();
        }
        #endregion
    }

    // Chase and Shoot are public so I can access them in the brain
    public void ChasePlayer()
    {
        //Checks if enemy is too close to player
        if (Vector2.Distance(transform.position, target.position) > stoppingDistance)
        {
            // Moves Enemy Character From Their Position to Target Position at set speed
            // Delta Time was chosen so the enemy speed isn't faster or slower depending on FPS
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }
    }

    public void ShootPlayer()
    {
        //Shooting Player Code
        if (fireTimer <= 0)
            {
                //spawns bullet and does firerate timer
                Instantiate(projectile, transform.position, Quaternion.identity);
                fireTimer = fireRate;
            }
            else
            {
                fireTimer -= Time.deltaTime;
            }
    }
}
