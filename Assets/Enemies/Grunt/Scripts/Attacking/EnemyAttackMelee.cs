using System.Collections;
using UnityEngine;

public class EnemyAttackMelee : MonoBehaviour
{
    private EnemyStats stats;
    private bool canAttack = true;
    private bool isAttacking = false;

    private Rigidbody2D rb;
    private Transform player;
    private Animator animator;

    public float mDamage;
    public float mFireRate;
    public float mFireCooldown;

    private PistolPerks pistolPerks;
    public Pistol pistol;
    private ShotgunPerks shotgunPerks;
    public Shotgun shotgun;
    private ARPerks arPerks;
    public AR ar;



    [SerializeField] private PerkDefinition perkThorns;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator = GetComponent<Animator>();

        stats = GetComponent<EnemyStats>(); // ← add this
        if (stats != null)
        {
            mDamage = stats.damage;
            mFireRate = stats.fireRate;
            mFireCooldown = stats.fireCooldown;
        }
        else
        {
            Debug.LogWarning("EnemyStats component not found on " + gameObject.name);
        }
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            pistol = player.GetComponentInChildren<Pistol>();
            pistolPerks = pistol.GetComponent<PistolPerks>();
            shotgun = player.GetComponentInChildren<Shotgun>();
            shotgunPerks = shotgun.GetComponent<ShotgunPerks>();
            ar = player.GetComponentInChildren<AR>();
            arPerks = ar.GetComponent<ARPerks>();
        }
        else
        {
            Debug.LogWarning("EnemyAttackMelee: Could not find Player in scene");
        }
    }

    public void TryAttack()
    {
        // Check if the enemy can attack and is not currently attacking
        if (!canAttack || isAttacking) return;

        // Start the attack coroutine
        StartCoroutine(HitCoroutine());
    }

    private IEnumerator HitCoroutine()
    {
        transform.localScale = new Vector3(3,3,3);
        animator.SetTrigger("Attacking");
        isAttacking = true;
        canAttack = false;
        rb.linearVelocity = Vector2.zero;

        var stats = player.GetComponent<PlayerMovement>();
        if (stats != null)
        {
            // Always damage the player
            stats.DamagePlayer(mDamage);

            // If thorns is active, reflect damage back to the attacker
            if (WeaponManager.Instance.HasPerk(perkThorns))
            {
                var damageHandler = GetComponent<DamageHandler>();
                if (damageHandler != null)
                {
                    damageHandler.TakeDamage(mDamage);
                    Debug.Log($"Thorns reflected {mDamage} damage back to {gameObject.name}");
                }
            }
        }
            
        yield return new WaitForSeconds(mFireRate);
        isAttacking = false;
        transform.localScale = new Vector3(2, 2, 2);

        yield return new WaitForSeconds(mFireCooldown);
        canAttack = true;
    }
}