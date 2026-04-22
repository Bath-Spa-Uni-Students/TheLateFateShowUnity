using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PistolPerks : MonoBehaviour
{
    [SerializeField] private Pistol pistol;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private GameObject PistolBullet;

    [Header("Hit Reload Perk")]
    [SerializeField] bool hitReload = false;
    [SerializeField] int hitReloadChance;

    [Header("Life Steal Perk")]
    [SerializeField] bool lifeSteal = false;
    [SerializeField] float lifeStealAmount;

    [Header("Pierce Perk")]
    [SerializeField] public bool pierce = false;

    [Header("Ricochet Perk")]
    [SerializeField] public bool ricochet = false;

    [Header("Scatter Perk")]
    [SerializeField] public bool scatter = false;
    [SerializeField] int numBullets = 6;
    [SerializeField] int bulletSpeed = 2;
    [SerializeField] float angleSpread = 260f;


    [Header("Crit Chance Perk")]
    [SerializeField] public bool critChance = false;
    [SerializeField][Range(0, 100)] int critChancePercent = 20;

    [Header("Poison Rounds Perk")]
    [SerializeField] public bool poisonRounds = false;
    [SerializeField] float poisonDamagePerTick = 5f;
    [SerializeField] float poisonDuration = 3f;
    [SerializeField] float poisonTickRate = 0.5f;

    [Header("Slow Rounds Perk")]
    [SerializeField] public bool slowRounds = false;
    [SerializeField] float slowAmount = 0.5f; 
    [SerializeField] float slowDuration = 2f;

    [Header("Power Cell Perk")]
    [SerializeField] public bool powerCell = false;
    [SerializeField] float powerCellDamageMultiplier = 1.25f;

    [Header("Speed Cell Perk")]
    [SerializeField] bool speedCell = false;
    [SerializeField][Range(0, 100)] int speedCellChance = 15;
    [SerializeField] float speedCellFireRateMultiplier = 2f;
    [SerializeField] float speedCellDuration = 2f;
    private bool speedCellActive = false;


    [Header("Shockwave Loader")]
    [SerializeField] bool shockwaveLoader = false;
    [SerializeField][Range(0, 100)] int shockwaveChance = 25;
    [SerializeField] float knockbackForce = 5f;





    // HitReload perk
    public void HitReloadPerk()
    {
        if (hitReload)
        {
            // Roll random number
            float roll = Random.Range(0, 100);
            Debug.Log($"random number rolled: {roll}");

            // If the roll number is the same as the hit chance
            if (roll <= hitReloadChance)
            {
                // Add 1 bullet to clip
                pistol.ammo += 1;
            }
        }
    }

    // LifeSteal perk
    public void LifeStealPerk()
    {
        if (lifeSteal)
        {
            // Get health %
            float heal = player.maxHealth * lifeStealAmount;

            // Heal player
            player.health += heal;
            Debug.Log($"Player healed for {heal} health!");

            if (player.health > player.maxHealth)
            {
                player.health = player.maxHealth; 
            }
        }
    }

    // Scatter perk

    public void ScatterBullet()
    {
        if (scatter)
        {
            // Get scatter spread
            float spread = angleSpread / numBullets;
            
            // Add bullets to scatter
            for (int i = 0; i < numBullets; i++)
            {
                float angle = i * spread;
                float rad = angle * Mathf.Deg2Rad;

                Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                GameObject bullet = Instantiate(PistolBullet, transform.position, Quaternion.identity);
                
                // Get rigid body of bullet
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                // Add force to scatter bullets
                rb.linearVelocity = direction * bulletSpeed;
            }
        }
    }





    // Crit Chance perk
    public float ApplyCrit(float baseDamage)
    {
        if (!critChance) return baseDamage;
        return Random.Range(0, 100) <= critChancePercent ? baseDamage * 2f : baseDamage;
    }

    // Poison Rounds perk
    public void ApplyPoisonRounds(DamageHandler enemy)
    {
        if (!poisonRounds || enemy == null) return;
        enemy.StartPoison(poisonDamagePerTick, poisonDuration, poisonTickRate);
    }
    //slow rounds perk
    public void ApplySlowRounds(DamageHandler enemy)
    {
        if (!slowRounds || enemy == null) return;
        enemy.StartSlow(slowAmount, slowDuration);
    }
    //power cell perk
    public float ApplyPowerCell(float baseDamage)
    {
        return powerCell ? baseDamage * powerCellDamageMultiplier : baseDamage;
    }

    // Speed Cell chance on shot to temporarily double fire rate
    public void ApplySpeedCell()
    {
        if (!speedCell || speedCellActive) return;
        if (Random.Range(0, 100) > speedCellChance) return;
        StartCoroutine(SpeedCellCoroutine());
    }

    private IEnumerator SpeedCellCoroutine()
    {
        speedCellActive = true;
        float original = pistol.fireRate;
        pistol.fireRate /= speedCellFireRateMultiplier; // lower value = faster fire
        yield return new WaitForSeconds(speedCellDuration);
        pistol.fireRate = original;
        speedCellActive = false;
    }

    // Shockwave Loader — chance to knock back enemy on hit
    public void ApplyShockwaveLoader(GameObject enemyObject)
    {
        if (!shockwaveLoader) return;
        if (Random.Range(0, 100) > shockwaveChance) return;

        Rigidbody2D enemyRb = enemyObject.GetComponent<Rigidbody2D>();
        if (enemyRb == null) return;

        Vector2 knockbackDir = (enemyObject.transform.position - player.transform.position).normalized;
        enemyRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
    }
  

    //Debugging perks in editor
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { speedCell = true; Debug.Log("Perk ON: Speed Cell"); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { shockwaveLoader = true; Debug.Log("Perk ON: Shockwave Loader"); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { critChance = true; Debug.Log("Perk ON: Crit Chance"); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { poisonRounds = true; Debug.Log("Perk ON: Poison"); }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { pierce = true; Debug.Log("Perk ON: Pierce"); }
        if (Input.GetKeyDown(KeyCode.Alpha6)) { ricochet = true; Debug.Log("Perk ON: Ricochet"); }
        if (Input.GetKeyDown(KeyCode.Alpha7)) { slowRounds = true; Debug.Log("Perk ON: Slow Rounds"); }
        if (Input.GetKeyDown(KeyCode.Alpha8)) { powerCell = true; Debug.Log("Perk ON: Power Cell"); }
        if (Input.GetKeyDown(KeyCode.Alpha9)) { ListPerks(); }
        if (Input.GetKeyDown(KeyCode.Alpha0)) { ResetPerks(); Debug.Log("All perks reset"); }
    }
    void ListPerks()
    {
        Debug.Log($"Current active Perks:\n" +
            $"SpeedCell: {speedCell}\n" +
            $"Shockwave Loader: {shockwaveLoader}\n" +
            $"Crit Chance: {critChance}\n" +
            $"Poison Rounds: {poisonRounds}\n" +
            $"Pierce: {pierce}\n" +
            $"Ricochet: {ricochet}\n" +
            $"SlowRounds: {slowRounds}\n" +
            $"Power Cell: {powerCell}");
    }
    void ResetPerks()
    {
        lifeSteal = false; hitReload = false; critChance = false;
        poisonRounds = false; pierce = false; ricochet = false; slowRounds = false; powerCell = false;
    }

}