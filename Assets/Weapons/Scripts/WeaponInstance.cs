using System.Collections.Generic;
using UnityEngine;
public enum WeaponType
{
    Pistol,
    AR,
    Shotgun
}

// Represents a weapon with its currently attached perks.
// This is the data object passed between chests, the WeaponManager, and UI.
[System.Serializable]
public class WeaponInstance
{
    public const int MAX_PERKS = 4;

    public WeaponType weaponType;
    public List<PerkDefinition> perks = new List<PerkDefinition>();

    public WeaponInstance(WeaponType type)
    {
        weaponType = type;
    }

    //Will also handle perk checks for adding to the weapon instance

    // Returns true if this weapon can accept more perks
    public bool HasPerkSlot => perks.Count < MAX_PERKS;

    // Returns true if this weapon already has the given perk
    public bool HasPerk(PerkDefinition perk) => perks.Contains(perk);

    // Tries to add a perk
    // Returns false if already at cap or already has perk
    public bool TryAddPerk(PerkDefinition perk)
    {
        if (!HasPerkSlot || HasPerk(perk)) return false;
        perks.Add(perk);
        return true;
    }

    // Replaces an existing perk at the given index with a new one
    public void ReplacePerk(int index, PerkDefinition newPerk)
    {
        if (index < 0 || index >= perks.Count) return;
        perks[index] = newPerk;
    }

    // Returns a copy of this WeaponInstance
    public WeaponInstance Clone()
    {
        WeaponInstance copy = new WeaponInstance(weaponType);
        copy.perks.AddRange(perks);
        return copy;
    }

}

