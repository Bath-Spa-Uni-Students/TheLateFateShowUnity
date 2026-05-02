using UnityEngine;

public class ScatterBullet : MonoBehaviour
{
    public float damage = 10f;
    [SerializeField] float lifetime = 2f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            var enemy = collision.GetComponent<DamageHandler>();
            if (enemy != null) enemy.TakeDamage(damage);
            Destroy(gameObject);
        }

        Debug.Log($"ScatterBullet physics hit: {collision.gameObject.name}");
    }
}