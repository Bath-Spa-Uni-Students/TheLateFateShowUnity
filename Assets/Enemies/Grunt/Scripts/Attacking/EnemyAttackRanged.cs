using System.Collections;
using UnityEngine;

public class EnemyAttackRanged : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    private bool canShoot = true;
    private EnemyStats stats;

    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
    }

    public void TryAttack(Transform player)
    {
        // Check if the enemy can shoot and if the player reference is valid
        if (!canShoot || player == null) return;
        StartCoroutine(Shoot(player));
    }

    private IEnumerator Shoot(Transform player)
    {
        canShoot = false;

        Vector2 dir = (player.position - firePoint.position).normalized;

        // Instantiate projectile
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // Give projectile a velocity
        proj.GetComponent<Rigidbody2D>().linearVelocity = dir * 10f;

        // Pass damage info to projectile
        var bullet = proj.GetComponent<Projectile>();
        if (bullet != null)
            bullet.SetDamage(stats.damage);

        yield return new WaitForSeconds(stats.fireCooldown);
        canShoot = true;
    }
}