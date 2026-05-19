using System;
using System.Collections.Generic;
using UnityEngine;

// Controls the player's current weapon and perks.
// All other systems (Pistol, AR, Shotgun, Chest, UI) talk to this.

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

    [SerializeField] private bool debugWeaponManager = false;

    public WeaponInstance CurrentWeapon { get; private set; }

    // Events other systems can subscribe to
    public event Action<WeaponInstance> OnWeaponChanged;
    public event Action<WeaponInstance> OnPerksChanged;
    public event Action<PerkDefinition, List<PerkDefinition>, Action<int>> OnPerkSwapRequired;

    // Chest events ChestUI subscribes to these
    public event Action<WeaponInstance, List<PerkDefinition>> OnChestNewGun;
    public event Action<List<PerkDefinition>> OnChestMergePerk;

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

    // CHEST LOGIC

    // Called by ChestObject when the player opens a chest.
    //
    // DIFFERENT weapon type:
    //   Show New Gun UI. Player picks a carry-over perk (or gets random), or keeps current gun.
    //
    // SAME weapon type:
    //   Auto-merge all non-duplicate chest perks if slots are free. No UI.
    //   If any perk has no room, show Merge Perk UI for those perks only.
    public void ReceiveChestWeapon(WeaponInstance chestWeapon)
    {
        if (debugWeaponManager)
        {
            Debug.Log($"[WeaponManager] Chest opened | player: {CurrentWeapon.weaponType} ({CurrentWeapon.perks.Count} perks) | chest: {chestWeapon.weaponType} ({chestWeapon.perks.Count} perks)");
            foreach (PerkDefinition p in CurrentWeapon.perks)
                Debug.Log($"  - {p.perkName} | compatible with chest weapon: {p.IsCompatibleWith(chestWeapon.weaponType)}");
        }

        if (chestWeapon.weaponType != CurrentWeapon.weaponType)
        {
            //DIFFERENT WEAPON TYPE 
            // Find perks on the player's current gun that could carry over.
            List<PerkDefinition> compatible = CurrentWeapon.perks.FindAll(
                p => p.IsCompatibleWith(chestWeapon.weaponType) && !chestWeapon.HasPerk(p));

            // Raise event — ChestUI shows the swap screen.
            OnChestNewGun?.Invoke(chestWeapon, compatible);
        }
        else
        {
            //SAME WEAPON TYPE
            // Filter out perks the player already has.
            List<PerkDefinition> newPerks = chestWeapon.perks.FindAll(p => !CurrentWeapon.HasPerk(p));

            if (newPerks.Count == 0)
            {
                if (debugWeaponManager)
                    Debug.Log("[WeaponManager] Same weapon all chest perks are duplicates. Nothing to do.");
                return;
            }

            // Silently add as many perks as free slots allow.
            List<PerkDefinition> autoAdded = new List<PerkDefinition>();
            List<PerkDefinition> overflow = new List<PerkDefinition>();

            foreach (PerkDefinition perk in newPerks)
            {
                if (CurrentWeapon.HasPerkSlot)
                {
                    CurrentWeapon.TryAddPerk(perk);
                    autoAdded.Add(perk);
                }
                else
                {
                    overflow.Add(perk);
                }
            }

            if (autoAdded.Count > 0)
            {
                OnPerksChanged?.Invoke(CurrentWeapon);
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.uiConfirm, Vector3.zero);

                if (debugWeaponManager)
                    Debug.Log($"[WeaponManager] Auto-merged {autoAdded.Count} perk(s) from same-type chest.");
            }

            // If perks couldn't fit, show the merge UI for the leftovers.
            if (overflow.Count > 0)
            {
                if (debugWeaponManager)
                    Debug.Log($"[WeaponManager] {overflow.Count} perk(s) couldn't auto-fit — opening merge UI.");

                OnChestMergePerk?.Invoke(overflow);
            }
        }
    }

    // CONFIRM ACTIONS

    // Player chose to take the new gun from a chest.
    // carriedPerk: a compatible perk carried over from the old gun, or null for a random one.
    public void ConfirmWeaponSwap(WeaponInstance chestWeapon, PerkDefinition carriedPerk)
    {
        // Start fresh with the chest weapon's type, keeping its pre-attached perks.
        WeaponInstance incoming = new WeaponInstance(chestWeapon.weaponType);

        foreach (PerkDefinition perk in chestWeapon.perks)
            incoming.TryAddPerk(perk);

        // Now try to add the carried (or random) perk on top, if there's room.
        if (incoming.HasPerkSlot)
        {
            if (carriedPerk != null && !incoming.HasPerk(carriedPerk))
            {
                incoming.TryAddPerk(carriedPerk);

                if (debugWeaponManager)
                    Debug.Log($"[WeaponManager] Carried perk: {carriedPerk.perkName}");
            }
            else if (carriedPerk == null)
            {
                // No compatible carry-over — give a random perk valid for the NEW weapon type.
                PerkDefinition random = GetRandomValidPerkForType(chestWeapon.weaponType, incoming.perks);
                if (random != null)
                {
                    incoming.TryAddPerk(random);

                    if (debugWeaponManager)
                        Debug.Log($"[WeaponManager] Random perk assigned: {random.perkName}");
                }
            }
        }

        CurrentWeapon = incoming;
        ActivateWeaponObject(CurrentWeapon.weaponType);
        OnWeaponChanged?.Invoke(CurrentWeapon);
        OnPerksChanged?.Invoke(CurrentWeapon);

        if (debugWeaponManager)
            Debug.Log($"[WeaponManager] Swapped to {CurrentWeapon.weaponType} with {CurrentWeapon.perks.Count} perk(s)");
    }

    // Player picked a perk to merge from the chest (same-weapon overflow, or perk swap).
    // swapOutIndex: index of existing perk to replace, or -1 to discard the incoming perk.
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
            // swapOutIndex == -1 means player discarded — do nothing.
        }

        OnPerksChanged?.Invoke(CurrentWeapon);

        if (debugWeaponManager)
            Debug.Log($"[WeaponManager] Merged perk: {chosen.perkName} | swapOutIndex: {swapOutIndex}");
    }


    // LEVEL-UP PERK

    public void ReceiveLevelUpPerk(PerkDefinition perk)
    {
        MergePerk(perk);
    }

    // HELPERS

    public bool HasPerk(PerkDefinition perk)
    {
        return CurrentWeapon != null && CurrentWeapon.HasPerk(perk);
    }

    // Returns a random perk compatible with the player's CURRENT weapon that isn't already held.
    public PerkDefinition GetRandomValidPerk()
    {
        return GetRandomValidPerkForType(CurrentWeapon.weaponType, CurrentWeapon.perks);
    }

    // Returns a random perk compatible with a SPECIFIC weapon type, excluding already-held perks.
    private PerkDefinition GetRandomValidPerkForType(WeaponType type, List<PerkDefinition> exclude)
    {
        List<PerkDefinition> valid = allPerks.FindAll(p =>
            p.IsCompatibleWith(type) && !exclude.Contains(p));

        if (valid.Count == 0) return null;
        return valid[UnityEngine.Random.Range(0, valid.Count)];
    }

    public List<PerkDefinition> GetRandomPerkSelection(int count)
    {
        List<PerkDefinition> valid = allPerks.FindAll(p =>
            p.IsCompatibleWith(CurrentWeapon.weaponType) && !CurrentWeapon.HasPerk(p));

        // Shuffle
        for (int i = valid.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (valid[i], valid[j]) = (valid[j], valid[i]);
        }

        return valid.GetRange(0, Mathf.Min(count, valid.Count));
    }

    private void MergePerk(PerkDefinition incoming)
    {
        if (CurrentWeapon.HasPerk(incoming))
        {
            if (debugWeaponManager) Debug.Log($"[WeaponManager] Already have perk: {incoming.perkName}, skipping.");
            return;
        }

        if (CurrentWeapon.HasPerkSlot)
        {
            CurrentWeapon.TryAddPerk(incoming);
            OnPerksChanged?.Invoke(CurrentWeapon);
            if (debugWeaponManager) Debug.Log($"[WeaponManager] Added perk: {incoming.perkName}");
        }
        else
        {
            if (debugWeaponManager) Debug.Log($"[WeaponManager] At perk cap. Requesting swap UI for: {incoming.perkName}");
            OnPerkSwapRequired?.Invoke(incoming, CurrentWeapon.perks, (swapIndex) =>
            {
                if (swapIndex >= 0)
                {
                    CurrentWeapon.ReplacePerk(swapIndex, incoming);
                    OnPerksChanged?.Invoke(CurrentWeapon);
                    if (debugWeaponManager) Debug.Log($"[WeaponManager] Replaced perk at index {swapIndex} with {incoming.perkName}");
                }
                else
                {
                    if (debugWeaponManager) Debug.Log($"[WeaponManager] Player discarded incoming perk: {incoming.perkName}");
                }
            });
        }
    }

    private void ActivateWeaponObject(WeaponType type)
    {
        if (pistolObject)
        {
            pistolObject.SetActive(type == WeaponType.Pistol);
            var p = pistolObject.GetComponent<Pistol>();
            if (p) { p.enabled = true; p.canShoot = type == WeaponType.Pistol; }
        }
        if (arObject)
        {
            arObject.SetActive(type == WeaponType.AR);
            var a = arObject.GetComponent<AR>();
            if (a) { a.enabled = true; a.canShoot = type == WeaponType.AR; }
        }
        if (shotgunObject)
        {
            shotgunObject.SetActive(type == WeaponType.Shotgun);
            var s = shotgunObject.GetComponent<Shotgun>();
            if (s) { s.enabled = true; s.canShoot = type == WeaponType.Shotgun; }
        }

        if (debugWeaponManager)
            Debug.Log($"[WeaponManager] ActivateWeaponObject: {type}");
    }
}