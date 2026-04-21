using UnityEngine;
using static UnityEngine.LowLevelPhysics2D.PhysicsShape;

public class ARBullet : MonoBehaviour
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

    // Call ARPerks script
    //private ARPerks arPerks;
    public AR ar;

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
       // ARPerks = AR.GetComponent<ARPerks>();

        // Gets world coordintes
        mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        // Bullet shoots
        // Move in the direction the bullet is facing
        rb.linearVelocity = transform.right * bulletSpeed;
        direction = transform.right;

        /* If AR perks true
        if (ARPerks.pierce == true)
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
        Debug.Log($"OnCollisionEnter2D hit: {collision.gameObject.name} tag: {collision.gameObject.tag}");

        if (collision.gameObject.CompareTag("Enemy")) return;


        if (!ARPerks.ricochet)
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
                float damage = bulletDamage;
                damage = ARPerks.ApplyPowerCell(damage);
                damage = ARPerks.ApplyCrit(damage);



                enemy.TakeDamage(damage);
                ARPerks.HitReloadPerk();
                ARPerks.LifeStealPerk();
                ARPerks.ApplyPoisonRounds(enemy);
                ARPerks.ApplySlowRounds(enemy);
                ARPerks.ApplyShockwaveLoader(collision.gameObject);
            }

            if (!ARPerks.pierce) Destroy(gameObject); // <-- add this
        }*/
    }
}