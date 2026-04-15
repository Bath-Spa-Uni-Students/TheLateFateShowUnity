using System.Collections;
using UnityEngine;

public class BossMelee : MonoBehaviour
{
    [SerializeField] private GameObject attackBarrier;           // Optional barrier that appears during attacks

    private EnemyStats stats;
    private bool mCanAttack;
    private bool mIsAttacking;

    private Rigidbody2D rb;
    private Transform player;
    private Animator animator;

    private float mDamage;
    private float mFireRate;
    private float mFireCooldown;

    private BossBehaviour bossBehaviour;
    [SerializeField] private float mslamWaitTimer; // Time to wait after the attack animation before deactivating the barrier
    [SerializeField] private float mSlamAnimFinished; // Time to wait after the attack animation before allowing the next attack

    private void Awake()
    {
        attackBarrier.gameObject.SetActive(false);

        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator = GetComponent<Animator>();
        bossBehaviour = GetComponent<BossBehaviour>();

        stats = GetComponent<EnemyStats>();
        if (stats != null)
        {
            mDamage = stats.damage;
            mFireRate = stats.fireRate;
            mFireCooldown = stats.fireCooldown;
            mslamWaitTimer = stats.slamWaitTimer;
        }
        else
        {
            Debug.LogWarning("EnemyStats component not found on " + gameObject.name);
        }
    }
    public void TryAttack()
    {
        // Check if the enemy can attack and is not currently attacking
        if (!stats.canAttack || stats.isAttacking) return;

        // Start the attack coroutine
        StartCoroutine(HitCoroutine());
        stats.canAttack = mCanAttack;
    }

    private IEnumerator HitCoroutine()
    {
        attackBarrier.gameObject.SetActive(true);

        animator.SetTrigger("Attack");
        animator.SetBool("IsAttacking", true);

        stats.isAttacking = true;
        stats.canAttack = false;
        rb.linearVelocity = Vector2.zero;

        var playerRef = player.GetComponent<PlayerMovement>();

        yield return new WaitForSeconds(mSlamAnimFinished);

        bossBehaviour.CheckPlayerDistance();

        if (stats.canDamage)
        {
            if (stats != null)
                playerRef.DamagePlayer(mDamage); // Apply damage to the player
        }

        yield return new WaitForSeconds(mslamWaitTimer);

        attackBarrier.gameObject.SetActive(false);
        stats.isAttacking = false;
        animator.SetBool("IsAttacking", false);

        yield return new WaitForSeconds(mFireCooldown); // Wait for the cooldown before allowing the next attack
        stats.canAttack = true;
    }
}