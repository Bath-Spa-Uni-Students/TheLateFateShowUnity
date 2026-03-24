using System.Collections;
using UnityEngine;

public class EnemyAttackRanged : MonoBehaviour
{
    [SerializeField] private GameObject projectile;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireCooldown = 1f;

    private bool canShoot = true;

    public void TryAttack(Transform player)
    {
        if (!canShoot) return;

        StartCoroutine(Shoot(player));
    }

    private IEnumerator Shoot(Transform player)
    {
        canShoot = false;

        Vector2 dir = (player.position - firePoint.position).normalized;

        GameObject bullet = Instantiate(projectile, firePoint.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().linearVelocity = dir * 10f;

        yield return new WaitForSeconds(fireCooldown);

        canShoot = true;
    }
}