using UnityEngine;
using static UnityEngine.LowLevelPhysics2D.PhysicsShape;

public class PistolBullet : MonoBehaviour
{
    public System.Action OnHitEnemy;

    // Reference mouse position
    private Vector3 mousePos;
    private Vector2 direction;

    // Reference main camera
    private Camera mainCam;

    //Reference rigidbody
    private Rigidbody2D rb;

    private CircleCollider2D circleCollider;

    [SerializeField] float bulletSpeed = 10;
    [SerializeField] float bulletDamage = 2;

    [SerializeField] SortingLayer enemyLayer;

    // Call PistolPerks script
    private PistolPerks pistolPerks;
    public Pistol pistol;

    // How many times bullet bounces for ricochet rounds
    [SerializeField] int bounces = 2;

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

        // Bullet shoots
        // Move in the direction the bullet is facing
        rb.linearVelocity = transform.right * bulletSpeed;
        direction = transform.right;

        // If pistol perks true
        if (pistolPerks.pierce == true)
        {
            // Exclude layers
            circleCollider.excludeLayers = LayerMask.GetMask("Enemy", "Player", "Player Projectile", "Enemy Projectile");
        }
    }

    public void Ricochet(Vector2 direction)
    {
        this.direction = direction;
        rb.linearVelocity += direction * bulletSpeed;   
    }

    // Ricochet perk
    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Enemy"))
        {
            var enemy = collision.gameObject.GetComponent<DamageHandler>();
            if (enemy != null)
            {
                float damage = bulletDamage;
          
                damage = pistolPerks.ApplyCrit(damage);

                enemy.TakeDamage(damage);
                pistolPerks.HitReloadPerk();
                pistolPerks.LifeStealPerk();
                pistolPerks.ApplyPoisonRounds(enemy);
               
            }
        }

        if (!pistolPerks.ricochet)
        {
            Destroy(gameObject);
            return;
        }

        bounces--;
        if (bounces <= 0)
        {
            Destroy(gameObject);
            return;
        }

        var contact = collision.contacts[0];
        Vector2 reflected = Vector2.Reflect(direction.normalized, contact.normal);
        direction = reflected.normalized;
        rb.linearVelocity = direction * bulletSpeed;
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