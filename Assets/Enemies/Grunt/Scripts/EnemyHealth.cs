using UnityEngine;

public class EnemyHealth : MonoBehaviour
{

    // Health of enemy
    public float enemyHealth = 100f;

    public void TakeDamage(float damage)
    {
        // Enemy loses health
        enemyHealth -= damage;

        // Destroy enemy if health is less than 0
        if (enemyHealth <= 0)
        {
            Destroy(gameObject);
        }

    }
}
