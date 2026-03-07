using UnityEngine;

public class BulletScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private float speed;
    private float damage;
    private Vector2 directionToTarget;

    private Transform player;
    private Vector2 target;
    void Start()
    {
        damage = 10;
        player = GameObject.FindGameObjectWithTag("Player").transform;

        directionToTarget = (player.position - transform.position).normalized;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        MoveBullet();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player.GetComponent<PlayerStats>().DamagePlayer(damage);
            DestroyProjectile();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            DestroyProjectile();
        }
    }

    void MoveBullet()
    {
        transform.position += (Vector3)(directionToTarget * speed * Time.deltaTime);
    }

    void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}
