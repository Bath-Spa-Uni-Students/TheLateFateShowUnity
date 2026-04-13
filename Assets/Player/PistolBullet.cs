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

    [SerializeField] float bulletSpeed = 10;
    [SerializeField] float bulletDamage = 50;

    // Call PistolPerks script
    private PistolPerks pistolPerks;
    public Pistol pistol;

    // Start is called before the first frame update
    private void Start()
    {
        // Gets camera component
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

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

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            var enemy = collision.gameObject.GetComponent<DamageHandler>();

            if (enemy != null)
            {
                enemy.TakeDamage(bulletDamage); // Damage is applied here
                pistolPerks.HitReloadPerk();
                pistolPerks.LifeStealPerk();

                if (!pistolPerks.pierce)
                {
                    Destroy(gameObject);
                }

            }

        }
    }
}