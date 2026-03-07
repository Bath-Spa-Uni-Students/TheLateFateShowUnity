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

        target = new Vector2(player.position.x, player.position.y);
        directionToTarget = (player.position - transform.position).normalized;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        transform.position += (Vector3)(directionToTarget * speed * Time.deltaTime);

        if (transform.position.x == target.x && transform.position.y == target.y)
        {
           DestroyProjectile();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player.GetComponent<PlayerStats>().DamagePlayer(damage);
            DestroyProjectile();
        }
    }

    void MoveBullet()
    {

    }

    void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}
