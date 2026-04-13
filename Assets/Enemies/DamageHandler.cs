using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using Microlight.MicroBar;

public class DamageHandler : MonoBehaviour
{
    [SerializeField] private EnemyStats stats;
    [SerializeField] private MicroBar healthBar;
    private float enemyHealth;
    private float maxHealth;

    private void Start()
    {
        enemyHealth = stats.health;
        maxHealth = stats.maxHealth;
        healthBar.Initialize(stats.maxHealth);
    }

    public void TakeDamage(float damage)
    {
        // Enemy loses health
        stats.health = stats.health - damage;

        if (damage == 0)
        {
            Debug.Log("Damage is 0");
            return;
        }

        healthBar.UpdateBar(healthBar.CurrentValue - damage);

        // Destroy enemy if health is less than 0
        if (stats.health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
