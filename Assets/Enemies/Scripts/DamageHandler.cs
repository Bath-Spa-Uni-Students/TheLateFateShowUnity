using UnityEngine;
using Microlight.MicroBar;
using System.Collections;
using FMODUnity;

public class DamageHandler : MonoBehaviour
{
    [SerializeField] private EnemyStats stats;
    [SerializeField] private MicroBar healthBar;
    [SerializeField] private EventReference takeDamageSound;
    [SerializeField] private EventReference deathSound;

    private PlayerMovement playerMovement;
    private GameObject player;
    private float enemyHealth;
    private float maxHealth;

    private Animator animator;

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
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(float damage)
    {
        damage = ApplyDamageModifiers(damage);
        stats.health -= damage;
        healthBar.UpdateBar(healthBar.CurrentValue - damage);

        if (stats.health <= 0)
        {
            if (bossBehaviour != null)
            {
                bossBehaviour.OnBossDeath();
            }
            else
            {
                AudioManager.Instance.PlayOneShot(deathSound, transform.position);
                Destroy(gameObject);
            }
            playerMovement.AddFame(10);
            GetComponent<TutorialEnemy>()?.NotifyDeath();
        }
        else
        {
            // Play the take damage sound if still alive
            if (!takeDamageSound.IsNull)
                AudioManager.Instance.PlayOneShot(takeDamageSound, transform.position);
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
