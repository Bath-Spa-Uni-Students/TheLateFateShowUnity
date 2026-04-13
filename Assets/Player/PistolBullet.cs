using UnityEngine;

public class PistolBullet : MonoBehaviour
{

    public System.Action OnHitEnemy;

    // Reference mouse position
    private Vector3 mousePos;

    // Reference main camera
    private Camera mainCam;

    //Reference rigidbody
    private Rigidbody2D rb;

    private CircleCollider2D circleCollider;

    [SerializeField] float bulletSpeed = 10;
    [SerializeField] float bulletDamage = 50;

    [SerializeField] SortingLayer enemyLayer;

    // Call PistolPerks script
    private PistolPerks pistolPerks;
    public Pistol pistol;

    // Start is called before the first frame update
    private void Start()
    {
        // Gets camera component
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        circleCollider = GetComponent<CircleCollider2D>();

        // Gets rigidbody component
        rb = GetComponent<Rigidbody2D>();
        pistolPerks = pistol.GetComponent<PistolPerks>();

        // Gets world coordintes
        mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        // Bullet travels towards mouse cursor
        Vector3 direction = mousePos - transform.position;
        Vector3 rotation = transform.position - mousePos;

        // Bullet shoots
        // Move in the direction the bullet is facing
        rb.linearVelocity = transform.right * bulletSpeed;

        // If pistol perks true
        if (pistolPerks.pierce == true)
        {
            // Exclude layers
            circleCollider.excludeLayers = LayerMask.GetMask("Enemy", "Player", "Player Projectile", "Enemy Projectile");
        }
    }

    // Pierce perk
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            var collidedEnemy = collision.gameObject.GetComponent<DamageHandler>();

            if (collidedEnemy != null)
            {
                // If pierce is disabled
                if (!pistolPerks.pierce)
                {
                    Destroy(gameObject);
                }
            }
        }
        // Destroy game object
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Bullet damage
   private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            var enemy = collision.gameObject.GetComponent<DamageHandler>();

            if (enemy != null)
            {
                enemy.TakeDamage(bulletDamage); // Damage is applied here
                pistolPerks.HitReloadPerk();
                pistolPerks.LifeStealPerk();
            }

        }
    }
}