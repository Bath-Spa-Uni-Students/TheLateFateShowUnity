using System.Collections;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float fireRate = 0.15f;
    [SerializeField] private float fireCooldown = 0.5f;

    private bool canAttack = true;
    private bool isAttacking = false;

    private Rigidbody2D rb;
    private Transform player;

    private void Awake()
    {
        // Get references to the Rigidbody2D and the player's Transform
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
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
            stats.DamagePlayer(damage);

        yield return new WaitForSeconds(fireRate);

        isAttacking = false;

        yield return new WaitForSeconds(fireCooldown);
        canAttack = true;
    }
}