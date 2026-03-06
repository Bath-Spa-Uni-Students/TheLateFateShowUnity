using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{

    [SerializeField] private float speed;
    [SerializeField] private Transform target;
    [SerializeField] private float stoppingDistance = 0.5f;

    [SerializeField] private float fireRate;
    private float fireTimer;

    [SerializeField] private GameObject projectile;
    private Transform player;
    [SerializeField] private float detectionRadius;
    [SerializeField] private GameObject detectionCircle;

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
    void Update()
    {
        //Checks if player is in detection radius
        if (Vector2.Distance(transform.position, target.position) < detectionRadius)
        {
            //Checks if enemy is too close to player
            if (Vector2.Distance(transform.position, target.position) > stoppingDistance)
            {
                // Moves Enemy Character From Their Position to Target Position at set speed
                // Delta Time was chosen so the enemy speed isn't faster or slower depending on FPS
                transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
            }

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
}
