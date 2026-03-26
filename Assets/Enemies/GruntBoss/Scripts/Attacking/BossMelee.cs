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
    [SerializeField] private float mslamWaitTimer; // Time to wait after the attack animation before deactivating the barrier

    private void Awake()
    {
        attackBarrier.gameObject.SetActive(false);

        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator = GetComponent<Animator>();

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

        stats.isAttacking = true;
        stats.canAttack = false;
        rb.linearVelocity = Vector2.zero;

        var playerRef = player.GetComponent<PlayerMovement>();
        if (stats != null)
            playerRef.DamagePlayer(mDamage);

        yield return new WaitForSeconds(mFireRate);

        yield return new WaitForSeconds(mslamWaitTimer);

        attackBarrier.gameObject.SetActive(false);
        stats.isAttacking = false;

        yield return new WaitForSeconds(mFireCooldown);
        stats.canAttack = true;
    }
}