using System.Collections;
using UnityEngine;

public class BossMelee : MonoBehaviour
{
    private EnemyStats stats;
    private bool canAttack = true;
    private bool isAttacking = false;

    private Rigidbody2D rb;
    private Transform player;

    public float mDamage;
    public float mFireRate;
    public float mFireCooldown;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        stats = GetComponent<EnemyStats>(); // ← add this
        if (stats != null)
        {
            mDamage = stats.damage;
            mFireRate = stats.fireRate;
            mFireCooldown = stats.fireCooldown;
        }
        else
        {
            Debug.LogWarning("EnemyStats component not found on " + gameObject.name);
        }
    }
    public void TryAttack()
    {
        // Check if the enemy can attack and is not currently attacking
        if (!canAttack || isAttacking) return;

        // Start the attack coroutine
        StartCoroutine(HitCoroutine());
    }

    private IEnumerator HitCoroutine()
    {
        isAttacking = true;
        canAttack = false;
        rb.linearVelocity = Vector2.zero;

        var stats = player.GetComponent<PlayerMovement>();
        if (stats != null)
            stats.DamagePlayer(mDamage);

        yield return new WaitForSeconds(mFireRate);

        isAttacking = false;

        yield return new WaitForSeconds(mFireCooldown);
        canAttack = true;
    }
}