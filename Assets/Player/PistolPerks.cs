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
    [SerializeField] float lifeStealAmmount;

    // HitReload function
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

    public void LifeStealPerk()
    {
        if (lifeSteal)
        {
        }
    }
}