using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    // Reference mouse position
    private Vector3 mousePos;

    // Reference main camera
    private Camera mainCam;

    //Reference rigidbody
    private Rigidbody2D rb;

    // Bullet attributes
    public float bulletSpeed;
    public float bulletDamage;

    // Start is called before the first frame update
    private void Start()
    {

        // Gets camera component
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        // Gets rigidbody component
        rb = GetComponent<Rigidbody2D>();

        // Gets world coordintes
        mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        // Bullet travels towards mouse cursor
        Vector3 direction = mousePos - transform.position;
        Vector3 rotation = transform.position - mousePos;

        // Bullet shoots
        rb.linearVelocity = new Vector2 (direction.x, direction.y).normalized * bulletSpeed;
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Get EnemyHealth script
        EnemyHealth enemy = collision.gameObject.GetComponent<EnemyHealth>();

        // Did the bullet hit an enemy?
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Take bullet damage
            enemy.TakeDamage(bulletDamage);
        }

        Destroy(gameObject);
    }
}