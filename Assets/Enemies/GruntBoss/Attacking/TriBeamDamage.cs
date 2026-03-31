using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class TriBeamDamage : MonoBehaviour

{
    private bool canShoot = true;
    private Rigidbody2D rb;
    private Transform player;
    private Animator animator;
    private BossBehaviour bossBehaviour;
    private EnemyStats stats;
    private GameObject attackBarrier;

    [SerializeField] private EnemyStats enemyStats;
    [SerializeField] private GameObject gruntBoss;
    [SerializeField] private GameObject parentBeam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator = GetComponent<Animator>();
        bossBehaviour = GetComponent<BossBehaviour>();

        stats = GetComponent<EnemyStats>();

        var playerRef = player.GetComponent<PlayerMovement>();

        enemyStats = gruntBoss.GetComponent<EnemyStats>();
        if (enemyStats != null)
        {
            Debug.Log("TriBeamDamage found EnemyStats component.");

        }
        else
        {
            Debug.LogError("TriBeamDamage could not find EnemyStats component in parent.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var playerRef = player.GetComponent<PlayerMovement>();

        if (collision.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.GetComponent<PlayerStats>();

            if (playerRef != null && !playerStats.isInvulnerable)
            {
                Debug.Log("TriBeamDamage hit player");
                playerRef.DamagePlayer(enemyStats.damage); // Apply damage to the player

                //playerStats.DamagePlayer(enemyStats.damage);
            }
            else if (playerRef == null)
            {
                Debug.Log("Player Stats does not exist, no damage taken.");
            }
            else
            {
                Debug.Log("Unknown Error");
            }
        }
    }
}
