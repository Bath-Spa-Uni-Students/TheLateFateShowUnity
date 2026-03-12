using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    private Vector3 mousePos;
    private Camera mainCam;
    private Rigidbody2D rb;
    public float bulletSpeed;
    public float bulletDamage;

    // Start is called before the first frame update
    private void Start()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        rb = GetComponent<Rigidbody2D>();
        mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        Vector3 direction = mousePos - transform.position;
        Vector3 rotation = transform.position - mousePos;

        rb.linearVelocity = new Vector2 (direction.x, direction.y).normalized * bulletSpeed;
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        EnemyHealth enemy = collision.gameObject.GetComponent<EnemyHealth>();

        if (collision.gameObject.CompareTag("Enemy"))
        {
            enemy.TakeDamage(bulletDamage);
        }

        Destroy(gameObject);
    }
}