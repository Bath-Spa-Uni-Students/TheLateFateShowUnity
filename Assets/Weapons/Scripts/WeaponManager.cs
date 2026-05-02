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

    public event Action<WeaponInstance, List<PerkDefinition>> OnChestNewGun;
    public event Action<List<PerkDefinition>> OnChestMergePerk;




    //when a chest is opened
    // If same weapon type: merge perks
    // If different weapon type swap immediately keep 1 valid perk.
    public void ReceiveChestWeapon(WeaponInstance chestWeapon)
    {
        if (chestWeapon.weaponType != CurrentWeapon.weaponType)
        {
            // Different weapon find compatible carry-over perks
            List<PerkDefinition> compatible = CurrentWeapon.perks.FindAll(
                p => p.IsCompatibleWith(chestWeapon.weaponType));

            OnChestNewGun?.Invoke(chestWeapon, compatible);
        }
        else
        {
            // Same weapon
            List<PerkDefinition> chestPerks = chestWeapon.perks;

            if (chestPerks.Count == 1 && CurrentWeapon.HasPerkSlot)
            {
                // No need for decision UI, just add the perk if it's not a duplicate
                if (!CurrentWeapon.HasPerk(chestPerks[0]))
                {
                    CurrentWeapon.TryAddPerk(chestPerks[0]);
                    OnPerksChanged?.Invoke(CurrentWeapon);
                    if (debugWeaponMangager)
                        Debug.Log($"[WeaponManager] Auto-added perk: {chestPerks[0].perkName}");
                }
                else
                {
                    if (debugWeaponMangager)
                        Debug.Log($"[WeaponManager] Auto-add skipped — already have perk: {chestPerks[0].perkName}");
                }
            }
            else
            {
                // Multiple perks or no free slot show merge panel with non-duplicates as options
                List<PerkDefinition> available = chestPerks.FindAll(p => !CurrentWeapon.HasPerk(p));

                if (available.Count == 0)
                {
                    if (debugWeaponMangager)
                        Debug.Log("[WeaponManager] Chest perks all duplicates — nothing to merge.");
                    return;
                }

                OnChestMergePerk?.Invoke(available);
            }
        }
    }
    // Called by ChestUI when player confirms a weapon swap
    public void ConfirmWeaponSwap(WeaponInstance newWeapon, PerkDefinition carriedPerk)
    {
        WeaponInstance incoming = new WeaponInstance(newWeapon.weaponType);

        if (carriedPerk != null)
            incoming.TryAddPerk(carriedPerk);

        CurrentWeapon = incoming;
        ActivateWeaponObject(CurrentWeapon.weaponType);
        OnWeaponChanged?.Invoke(CurrentWeapon);
        OnPerksChanged?.Invoke(CurrentWeapon);

        if (debugWeaponMangager)
            Debug.Log($"[WeaponManager] Swapped to {CurrentWeapon.weaponType}. Carried perk: {carriedPerk?.perkName ?? "none"}");
    }
    // Called by ChestUI when player picks a perk to merge (with or without swap-out)
    public void ConfirmMergePerk(PerkDefinition chosen, int swapOutIndex = -1)
    {
        if (CurrentWeapon.HasPerkSlot)
        {
            CurrentWeapon.TryAddPerk(chosen);
        }
        else
        {
            if (swapOutIndex >= 0)
                CurrentWeapon.ReplacePerk(swapOutIndex, chosen);
            // if -1, player discarded — do nothing
        }

        OnPerksChanged?.Invoke(CurrentWeapon);

        if (debugWeaponMangager)
            Debug.Log($"[WeaponManager] Merged perk: {chosen.perkName} | swapOut: {swapOutIndex}");
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

   /* private void SwapWeapon(WeaponInstance newWeapon)
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
    }*/

private void MergePerk(PerkDefinition incoming)
    {
        if (CurrentWeapon.HasPerk(incoming))
        {
            // Already have this perk, do nothing
            if (debugWeaponMangager) Debug.Log($"[WeaponManager] Already have perk: {incoming.perkName}, skipping.");
            return;

        }

        if (CurrentWeapon.HasPerkSlot)
        {
            // Add perk directly
            CurrentWeapon.TryAddPerk(incoming);
            OnPerksChanged?.Invoke(CurrentWeapon);
            if (debugWeaponMangager) Debug.Log($"[WeaponManager] Added perk: {incoming.perkName}");

        }
        else
        {
            // if max perks PerkSwapUI can handle it
            if (debugWeaponMangager) Debug.Log($"[WeaponManager] At perk cap. Requesting swap UI for: {incoming.perkName}");
            OnPerkSwapRequired?.Invoke(incoming, CurrentWeapon.perks, (swapIndex) =>
            {
                if (swapIndex >= 0)
                {
                    // Replace perk at chosen index
                    CurrentWeapon.ReplacePerk(swapIndex, incoming);
                    OnPerksChanged?.Invoke(CurrentWeapon);
                    if (debugWeaponMangager) Debug.Log($"[WeaponManager] Replaced perk at index {swapIndex} with {incoming.perkName}");
                }
                else
                {
                    if (debugWeaponMangager) Debug.Log($"[WeaponManager] Player discarded incoming perk: {incoming.perkName}");
                }
            });
        }
    }

    private void ActivateWeaponObject(WeaponType type)
    {
        if (pistolObject) pistolObject.SetActive(type == WeaponType.Pistol);
        if (arObject) arObject.SetActive(type == WeaponType.AR);
        if (shotgunObject) shotgunObject.SetActive(type == WeaponType.Shotgun);
    }
}