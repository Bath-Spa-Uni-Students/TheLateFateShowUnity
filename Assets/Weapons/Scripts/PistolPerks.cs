using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PistolPerks : MonoBehaviour
{
    [SerializeField] private Pistol pistol;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private GameObject scatterBullet;

    [Header("Perk Definitions ")]// Assign these in the Inspector with the PerkDefinition ScriptableObjects
    [SerializeField] private PerkDefinition perkHitReload;
    [SerializeField] private PerkDefinition perkLifeSteal;
    [SerializeField] private PerkDefinition perkPierce;
    [SerializeField] private PerkDefinition perkRicochet;
    [SerializeField] private PerkDefinition perkScatter;
    [SerializeField] private PerkDefinition perkThorns;
    [SerializeField] private PerkDefinition perkCritChance;
    [SerializeField] private PerkDefinition perkPoisonRounds;
    [SerializeField] private PerkDefinition perkSlowRounds;
    [SerializeField] private PerkDefinition perkPowerCell;
    [SerializeField] private PerkDefinition perkSpeedCell;
    [SerializeField] private PerkDefinition perkShockwaveLoader;

    [Header("Perk Values (balance these)")]
    [SerializeField] int hitReloadChance = 25;
    [SerializeField] float lifeStealAmount = 0.05f;
    [SerializeField] int numBullets = 6;
    [SerializeField] float scatterBulletSpeed = 10f;
    [SerializeField] float angleSpread = 260f;
    [SerializeField][Range(0, 100)] int critChancePercent = 20;
    [SerializeField] float poisonDamagePerTick = 5f;
    [SerializeField] float poisonDuration = 3f;
    [SerializeField] float poisonTickRate = 0.5f;
    [SerializeField] float slowAmount = 0.5f;
    [SerializeField] float slowDuration = 2f;
    [SerializeField] float powerCellDamageMultiplier = 1.25f;
    [SerializeField][Range(0, 100)] int speedCellChance = 15;
    [SerializeField] float speedCellFireRateMultiplier = 2f;
    [SerializeField] float speedCellDuration = 2f;
    [SerializeField][Range(0, 100)] int shockwaveChance = 25;
    [SerializeField] float knockbackForce = 5f;

    private bool speedCellActive = false;

    //check if perks are active
    private bool Has(PerkDefinition perk) => perk != null && WeaponManager.Instance != null && WeaponManager.Instance.HasPerk(perk);

    public void HitReloadPerk()
    {
        if (!Has(perkHitReload)) return;
        if (Random.Range(0, 100) <= hitReloadChance)
            pistol.ammo += 1;
    }

    public void LifeStealPerk()
    {
        if (!Has(perkLifeSteal)) return;
        float heal = player.maxHealth * lifeStealAmount;
        player.health = Mathf.Min(player.health + heal, player.maxHealth);
    }

    public void ScatterBullet(float damage)
    {
        if (!Has(perkScatter)) return;
        float spread = angleSpread / numBullets;
        for (int i = 0; i < numBullets; i++)
        {
            float angle = i * spread;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            GameObject bullet = Instantiate(scatterBullet, transform.position, Quaternion.identity);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = direction * scatterBulletSpeed;
            var splitter = bullet.GetComponent<ScatterBullet>();
            if (splitter != null) splitter.damage = damage;
        }
    }

    public float ApplyCrit(float baseDamage)
    {
        if (!Has(perkCritChance)) return baseDamage;
        return Random.Range(0, 100) <= critChancePercent ? baseDamage * 2f : baseDamage;
    }

    public void ApplyPoisonRounds(DamageHandler enemy)
    {
        if (!Has(perkPoisonRounds) || enemy == null) return;
        enemy.StartPoison(poisonDamagePerTick, poisonDuration, poisonTickRate);
    }

    public void ApplySlowRounds(DamageHandler enemy)
    {
        if (!Has(perkSlowRounds) || enemy == null) return;
        enemy.StartSlow(slowAmount, slowDuration);
    }

    public float ApplyPowerCell(float baseDamage)
    {
        return Has(perkPowerCell) ? baseDamage * powerCellDamageMultiplier : baseDamage;
    }

    public void ApplySpeedCell()
    {
        if (!Has(perkSpeedCell) || speedCellActive) return;
        if (Random.Range(0, 100) > speedCellChance) return;
        StartCoroutine(SpeedCellCoroutine());
    }

    public void ApplyShockwaveLoader(GameObject enemyObject)
    {
        if (!Has(perkShockwaveLoader)) return;
        if (Random.Range(0, 100) > shockwaveChance) return;
        Rigidbody2D enemyRb = enemyObject.GetComponent<Rigidbody2D>();
        if (enemyRb == null) return;
        Vector2 dir = (enemyObject.transform.position - player.transform.position).normalized;
        enemyRb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
    }

    // Pierce and Ricochet are checked directly in Pistol/Bullet scripts
    public PerkDefinition PerkPierce => perkPierce;
    public PerkDefinition PerkRicochet => perkRicochet;
    public PerkDefinition PerkThorns => perkThorns;

    private IEnumerator SpeedCellCoroutine()
    {
        speedCellActive = true;
        float original = pistol.fireRate;
        pistol.fireRate /= speedCellFireRateMultiplier;
        yield return new WaitForSeconds(speedCellDuration);
        pistol.fireRate = original;
        speedCellActive = false;
    }

}