using UnityEngine;

[CreateAssetMenu(fileName = "NewPerk", menuName = "LateFateShow/Perk Definition")]
public class PerkDefinition : ScriptableObject
{
    [Header("Name")]
    public string perkName;
    [TextArea] public string description;

    [Header("Weapon Compatibility")]
    public bool compatibleWithPistol = true;
    public bool compatibleWithAR = true;
    public bool compatibleWithShotgun = true;

    // Returns true if this perk can be applied to the given weapon type.
    public bool IsCompatibleWith(WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Pistol => compatibleWithPistol,
            WeaponType.AR => compatibleWithAR,
            WeaponType.Shotgun => compatibleWithShotgun,
            _ => false
        };
    }
}
