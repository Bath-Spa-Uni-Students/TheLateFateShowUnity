using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using Microlight.MicroBar;
using System.Collections;

public class DamageHandler : MonoBehaviour
{
    [SerializeField] private EnemyStats stats;
    [SerializeField] private MicroBar healthBar;

    private PlayerMovement playerMovement;
    private GameObject player;
    private float enemyHealth;
    private float maxHealth;

    private BossBehaviour bossBehaviour;


    private Coroutine poisonCoroutine;

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
        Debug.Log("damaged " + damage);

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

    public void StartPoison(float damagePerTick, float duration, float tickRate)
    {
        // Don't stack
        if (poisonCoroutine != null) StopCoroutine(poisonCoroutine);
        poisonCoroutine = StartCoroutine(PoisonCoroutine(damagePerTick, duration, tickRate));
    }

    private IEnumerator PoisonCoroutine(float damagePerTick, float duration, float tickRate)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            yield return new WaitForSeconds(tickRate);
            TakeDamage(damagePerTick);
            elapsed += tickRate;
        }
    }
}
