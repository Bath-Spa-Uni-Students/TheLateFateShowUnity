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
    private EnemyBehaviour enemyBehaviour;  // Reference to check leader status

    private Coroutine poisonCoroutine;
    private Coroutine slowCoroutine;

    private void Start()
    {
        enemyHealth = stats.health;
        maxHealth = stats.maxHealth;
        healthBar.Initialize(stats.maxHealth);
        player = GameObject.FindGameObjectWithTag("Player");
        playerMovement = player.GetComponent<PlayerMovement>();
    }

    private void Awake()
    {
        enemyBehaviour = GetComponent<EnemyBehaviour>();
        bossBehaviour = GetComponent<BossBehaviour>();
    }

    public void TakeDamage(float damage)
    {

        Debug.Log($"enemyBehaviour is null: {enemyBehaviour == null}");

        if (enemyBehaviour != null)
            Debug.Log($"isLeader: {enemyBehaviour.isLeader} | followerCount: {enemyBehaviour.followerCount}");

        damage = ApplyDamageModifiers(damage);
        // Enemy loses health
        stats.health -= damage;
        Debug.Log("damaged " + damage);

        healthBar.UpdateBar(healthBar.CurrentValue - damage);
        Debug.Log($"{gameObject.name} took {damage} damage. Health remaining: {stats.health}");
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
            playerMovement.AddFame(10);
            Destroy(gameObject);
        }
    }

    private float ApplyDamageModifiers(float damage)
    {
        // Leader damage reduction if followers are alive
        if (enemyBehaviour != null
            && enemyBehaviour.isLeader
            && enemyBehaviour.followerCount > 0)
        {
            float reduction = 1f - stats.leaderDamageReduction;
            damage *= reduction;
            Debug.Log($"Leader damage reduction applied ({stats.leaderDamageReduction * 100}%). " +
                      $"Effective damage: {damage}");
        }

        return damage;
    }
    public void StartPoison(float damagePerTick, float duration, float tickRate)
    {
        // Don't stack
        if (poisonCoroutine != null) StopCoroutine(poisonCoroutine);
        poisonCoroutine = StartCoroutine(PoisonCoroutine(damagePerTick, duration, tickRate));
    }

    public void StartSlow(float slowMultiplier, float duration)
    {
        if (slowCoroutine != null) StopCoroutine(slowCoroutine);
        slowCoroutine = StartCoroutine(SlowCoroutine(slowMultiplier, duration));
    }

    private IEnumerator SlowCoroutine(float slowMultiplier, float duration)
    {
        var movement = GetComponent<EnemyStats>();
        if (movement == null) yield break;

        float original = movement.speed;
        movement.speed *= slowMultiplier;
        yield return new WaitForSeconds(duration);
        movement.speed = original;
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
