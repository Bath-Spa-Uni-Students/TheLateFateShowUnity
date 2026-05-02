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
}

