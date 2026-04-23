using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float damage;

    public void SetDamage(float value)
    {
        damage = value;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var player = collision.GetComponent<PlayerMovement>();
            if (player != null)
                player.DamagePlayer(damage); // Damage is applied here
        }

        Destroy(gameObject); // Destroy bullet after hit
    }
}