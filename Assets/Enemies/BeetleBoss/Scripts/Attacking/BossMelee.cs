using System.Collections;
using UnityEngine;

public class BossMelee : MonoBehaviour
{    [SerializeField] private GameObject attackBarrier;
 
    private EnemyStats stats;
    private Rigidbody2D rb;
    private Transform player;
    private Animator animator;
    private BossBehaviour bossBehaviour;
 
    private float mDamage;
    private float mFireCooldown;
    private float mSlamAnimFinished;  // Delay before hit-check (matches your attack animation's hit-frame)
    private float mslamWaitTimer;     // Additional hold time after hit before the barrier drops
 


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator = GetComponent<Animator>();
        bossBehaviour = GetComponent<BossBehaviour>();
        stats = GetComponent<EnemyStats>();

        if (attackBarrier != null)
            attackBarrier.SetActive(false);

        if (stats != null)
        {
            mDamage = stats.damage;
            mFireCooldown = stats.fireCooldown;
            mslamWaitTimer = stats.slamWaitTimer;
            // slamWaitTimer doubles as the "hit-frame delay" in your original code.
            // Split it in half so the hit lands mid-animation and the barrier lingers briefly.
            mSlamAnimFinished = stats.slamWaitTimer * 0.5f;
            mslamWaitTimer = stats.slamWaitTimer * 0.5f;
        }
        else
        {
            Debug.LogWarning("[BossMelee] EnemyStats not found on " + gameObject.name);
        }
    }
    public void TryAttack()
    {
        // Check if the enemy can attack and is not currently attacking
        if (!stats.canAttack || stats.isAttacking)
        {
            Debug.Log("Cannot attack: " + (stats.canAttack ? "Already attacking" : "Attack on cooldown"));
            return;
        }

        // Start the attack coroutine
        StartCoroutine(HitCoroutine());
    }

    private IEnumerator HitCoroutine()
    {
        //Setup 
        stats.isAttacking = true;
        stats.canAttack = false;
        rb.linearVelocity = Vector2.zero;

        if (attackBarrier != null) attackBarrier.SetActive(true);
        animator.SetTrigger("Attack");
        animator.SetBool("IsAttacking", true);

        //  Wait for the hit frame
        yield return new WaitForSeconds(mSlamAnimFinished);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.bossAttackMelee, transform.position);
        bossBehaviour.CheckPlayerDistance();

        if (stats.canDamage && player != null)
        {
            var playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
                playerMovement.DamagePlayer(mDamage);
        }

        // Hold the barrier briefly then clean up 
        yield return new WaitForSeconds(mslamWaitTimer);

        if (attackBarrier != null) attackBarrier.SetActive(false);
        stats.isAttacking = false;
        animator.SetBool("IsAttacking", false);

        // Cooldown
        yield return new WaitForSeconds(mFireCooldown);
        stats.canAttack = true;

    }
}