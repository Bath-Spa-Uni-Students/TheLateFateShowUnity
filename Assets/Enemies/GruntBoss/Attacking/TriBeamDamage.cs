using UnityEngine;

public class TriBeamDamage : MonoBehaviour

{
    [SerializeField] private EnemyStats enemyStats;
    [SerializeField] private GameObject gruntBoss;
    [SerializeField] private GameObject parentBeam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
        if (collision.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.GetComponent<PlayerStats>();

            if (playerStats != null && !playerStats.isInvulnerable)
            {
                Debug.Log("TriBeamDamage hit player");

                playerStats.DamagePlayer(enemyStats.damage);
            }
            else
            {
                Debug.Log("Player is invulnerable, no damage taken.");
            }
        }
    }
}
