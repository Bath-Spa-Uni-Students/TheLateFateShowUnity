using Unity.VisualScripting;
using UnityEngine;

public class PistolPerks : MonoBehaviour
{
    [SerializeField] private Pistol pistol;
    [SerializeField] private PlayerMovement player;

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

    [Header("Crit Chance Perk")]
    [SerializeField] public bool critChance = false;
    [SerializeField][Range(0, 100)] int critChancePercent = 20;

    [Header("Poison Rounds Perk")]
    [SerializeField] public bool poisonRounds = false;
    [SerializeField] float poisonDamagePerTick = 5f;
    [SerializeField] float poisonDuration = 3f;
    [SerializeField] float poisonTickRate = 0.5f;

    // HitReload perk
    public void HitReloadPerk()
    {
        if (hitReload)
        {
            // Roll random number
            float roll = Random.Range(0, 100);
            Debug.Log(roll);

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

    //Debugging perks in editor
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { lifeSteal = true; Debug.Log("Perk ON: Life Steal"); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { hitReload = true; Debug.Log("Perk ON: Hit Reload"); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { critChance = true; Debug.Log("Perk ON: Crit Chance"); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { poisonRounds = true; Debug.Log("Perk ON: Poison"); }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { pierce = true; Debug.Log("Perk ON: Pierce"); }
        if (Input.GetKeyDown(KeyCode.Alpha6)) { ricochet = true; Debug.Log("Perk ON: Ricochet"); }
        if(Input.GetKeyDown(KeyCode.Alpha9)) { ListPerks(); }
        if (Input.GetKeyDown(KeyCode.Alpha0)) { ResetPerks(); Debug.Log("All perks reset"); }
    }
    void ListPerks()
    {
        Debug.Log($"Current active Perks:\n" +
            $"Life Steal: {lifeSteal}\n" +
            $"Hit Reload:" + hitReload + $"\n" +
            $"Crit Chance: {critChance}\n" +
            $"Poison Rounds: {poisonRounds}\n" +
            $"Pierce: {pierce}\n" +
            $"Ricochet: {ricochet}");
    }
    void ResetPerks()
    {
        lifeSteal = false; hitReload = false; critChance = false;
        poisonRounds = false; pierce = false; ricochet = false;
    }

}