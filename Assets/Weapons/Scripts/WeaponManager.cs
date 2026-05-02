using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


// Controls the player's current weapon and perks.
// All other systems wiil (Pistol, AR, Shotgun, Chest, UI) talk to this.

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }

    [Header("Starting Weapon")]
    [SerializeField] private WeaponType startingWeapon = WeaponType.Pistol;

    [Header("Weapon GameObjects")]
    [SerializeField] private GameObject pistolObject;
    [SerializeField] private GameObject arObject;
    [SerializeField] private GameObject shotgunObject;

    [Header("All Perk Definitions (assign in Inspector)")]
    [SerializeField] private List<PerkDefinition> allPerks;

    [SerializeField] private bool debugWeaponMangager = false;
    // The players current weapon and its perks
    public WeaponInstance CurrentWeapon { get; private set; }

    // Events other systems can subscribe to
    public event Action<WeaponInstance> OnWeaponChanged;
    public event Action<WeaponInstance> OnPerksChanged;

    // WeaponManager raises this so PerkSwapUI can show.
    public event Action<PerkDefinition, List<PerkDefinition>, Action<int>> OnPerkSwapRequired;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        CurrentWeapon = new WeaponInstance(startingWeapon);
        ActivateWeaponObject(startingWeapon);
    }

    //when a chest is opened
    // If same weapon type: merge perks
    // If different weapon type swap immediately keep 1 valid perk.
    public void ReceiveChestWeapon(WeaponInstance chestWeapon)
    {
        if (chestWeapon.weaponType == CurrentWeapon.weaponType)
        {
            // Duplicate gun
            foreach (PerkDefinition perk in chestWeapon.perks)
                MergePerk(perk);
        }
        else
        {
            SwapWeapon(chestWeapon);
        }
    }

    public void ReceiveLevelUpPerk(PerkDefinition perk)
    {
        MergePerk(perk);
    }

    public bool HasPerk(PerkDefinition perk)
    {
        return CurrentWeapon != null && CurrentWeapon.HasPerk(perk);
    }

    public PerkDefinition GetRandomValidPerk()
    {
        List<PerkDefinition> valid = allPerks.FindAll(p =>
            p.IsCompatibleWith(CurrentWeapon.weaponType) &&
            !CurrentWeapon.HasPerk(p));

        if (valid.Count == 0) return null;
        return valid[UnityEngine.Random.Range(0, valid.Count)];
    }

    public List<PerkDefinition> GetRandomPerkSelection(int count)
    {
        List<PerkDefinition> valid = allPerks.FindAll(p =>p.IsCompatibleWith(CurrentWeapon.weaponType) && !CurrentWeapon.HasPerk(p));

        // Shuffle
        for (int i = valid.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            PerkDefinition tmp = valid[i];
            valid[i] = valid[j];
            valid[j] = tmp;
        }

        return valid.GetRange(0, Mathf.Min(count, valid.Count));
    }

    private void SwapWeapon(WeaponInstance newWeapon)
    {
        // Find 1 perk from old weapon that's valid on the new weapon
        PerkDefinition carried = null;
        foreach (PerkDefinition perk in CurrentWeapon.perks)
        {
            if (perk.IsCompatibleWith(newWeapon.weaponType))
            {
                carried = perk;
                break;
            }
        }

        // If nothing valid, assign a random perk for the new weapon
        if (carried == null)
        {
            // Temporarily set weapon so GetRandomValidPerk works correctly
            CurrentWeapon = newWeapon;
            carried = GetRandomValidPerk();
            if (carried != null) newWeapon.TryAddPerk(carried);
        }
        else
        {
            newWeapon.TryAddPerk(carried);
        }

        // Merge any perks that came on the chest weapon itself 
        while (newWeapon.perks.Count > WeaponInstance.MAX_PERKS)
            newWeapon.perks.RemoveAt(newWeapon.perks.Count - 1);

        CurrentWeapon = newWeapon;
        ActivateWeaponObject(CurrentWeapon.weaponType);
        OnWeaponChanged?.Invoke(CurrentWeapon);
        OnPerksChanged?.Invoke(CurrentWeapon);

        if (debugWeaponMangager) Debug.Log($"[WeaponManager] Swapped to {CurrentWeapon.weaponType}. Carried perk: {carried?.perkName ?? "none (random assigned)"}");
    }

private void MergePerk(PerkDefinition incoming)
    {
        if (CurrentWeapon.HasPerk(incoming))
        {
            // Already have this perk, do nothing
        }

        if (CurrentWeapon.HasPerkSlot)
        {
            // Add perk directly
        }
        else
        {
            // if max perks PerkSwapUI can handle it
            Debug.Log($"[WeaponManager] At perk cap. Requesting swap UI for: {incoming.perkName}");
           
            {
                if (swapIndex >= 0)
                {
                    // Replace perk at chosen index
                    CurrentWeapon.ReplacePerk(swapIndex, incoming);

                }
                else
                {
                    Debug.Log($"[WeaponManager] Player discarded incoming perk: {incoming.perkName}");
                }
            }
        }
    }

    private void ActivateWeaponObject(WeaponType type)
    {
        if (pistolObject) pistolObject.SetActive(type == WeaponType.Pistol);
        if (arObject) arObject.SetActive(type == WeaponType.AR);
        if (shotgunObject) shotgunObject.SetActive(type == WeaponType.Shotgun);
    }


}