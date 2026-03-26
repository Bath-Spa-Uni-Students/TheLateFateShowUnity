using UnityEngine;

public class PistolPerks : MonoBehaviour
{

    [SerializeField] bool hitReload = false;
    [SerializeField] float hitReloadChance = 0.5f;

    // HitReload function
     public void HitReloadPerk()
    {
        if (hitReload)
        {
            // Roll random number
            float roll = Random.value;

            // If the roll number is the same as the hit chance
            if (roll == hitReloadChance)
            {
                // Add 1 bullet to clip
                Pistol pistol = GetComponent<Pistol>();
                pistol.ammo += 1;
            }
        }
    }
}