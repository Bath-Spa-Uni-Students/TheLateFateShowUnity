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

    private void Start()
    {
        enemyHealth = stats.health;
        maxHealth = stats.maxHealth;
        healthBar.Initialize(stats.maxHealth);
        player = GameObject.FindGameObjectWithTag("Player");
        playerMovement = player.GetComponent<PlayerMovement>();
    }

    public void TakeDamage(float damage)
    {
        // Enemy loses health
        stats.health = stats.health - damage;
        Debug.Log("damaged");

        healthBar.UpdateBar(healthBar.CurrentValue - damage);

        // Destroy enemy if health is less than 0
        if (stats.health <= 0)
        {
            playerMovement.AddFame(1);
            Debug.Log("fame+");
            Destroy(gameObject);
        }
    }
}
