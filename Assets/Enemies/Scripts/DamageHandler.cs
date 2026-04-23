using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using Microlight.MicroBar;

public class DamageHandler : MonoBehaviour
{
    [SerializeField] private EnemyStats stats;
    [SerializeField] private MicroBar healthBar;

    private PlayerMovement playerMovement;
    private GameObject player;
    private float enemyHealth;
    private float maxHealth;

    private BossBehaviour bossBehaviour;

    private void Start()
    {
        enemyHealth = stats.health;
        maxHealth = stats.maxHealth;
        healthBar.Initialize(stats.maxHealth);
        player = GameObject.FindGameObjectWithTag("Player");
        playerMovement = player.GetComponent<PlayerMovement>();
        bossBehaviour = GetComponent<BossBehaviour>(); // will be null on non-boss enemies
    }

    public void TakeDamage(float damage)
    {
        // Enemy loses health
        stats.health = stats.health - damage;
        Debug.Log("damaged");

        healthBar.UpdateBar(healthBar.CurrentValue - damage);

        if (stats.health <= 0)
        {
            if (bossBehaviour != null)
            {
                bossBehaviour.OnBossDeath();
            }
            else
            {
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.gruntDeath, transform.position);
            }
            playerMovement.AddFame(100);
            Debug.Log("fame+");
            Destroy(gameObject);
        }
    }

  
}
