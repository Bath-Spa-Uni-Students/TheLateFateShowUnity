using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class DamageHandler : MonoBehaviour
{
    [SerializeField] private EnemyStats stats;
    public void TakeDamage(float damage)
    {
        // Enemy loses health
        stats.health = stats.health - damage;
        Debug.Log("damaged");

        // Destroy enemy if health is less than 0
        if (stats.health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
