using UnityEngine;

public class PistolPerks : MonoBehaviour
{

    [SerializeField] bool hitReload = false;
    [SerializeField] int hitReloadChance;
    [SerializeField] private Pistol pistol;

    private void Start()
    {
    }

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
}