using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    // Weapons
    [field: Header("Weapon SFX")]
    [field: SerializeField] public EventReference weaponPickup { get; private set; }

    [field: Header("Pistol SFX")]
    [field: SerializeField] public EventReference pistolShoot { get; private set; }
    [field: SerializeField] public EventReference pistolReload { get; private set; }
    [field: SerializeField] public EventReference pistolNoAmmo { get; private set; }

    [field: Header("AR SFX")]
    [field: SerializeField] public EventReference arShoot { get; private set; }
    [field: SerializeField] public EventReference arReload { get; private set; }
    [field: SerializeField] public EventReference arNoAmmo { get; private set; }

    [field: Header("Shotgun SFX")]
    [field: SerializeField] public EventReference shotgunShoot { get; private set; }
    [field: SerializeField] public EventReference shotgunReload { get; private set; }
    [field: SerializeField] public EventReference shotgunNoAmmo { get; private set; }

    // Player

    [field: Header("Player SFX")]
    [field: SerializeField] public EventReference playerFootsteps { get; private set; }
    [field: SerializeField] public EventReference playerHurt { get; private set; }
    [field: SerializeField] public EventReference playerDeath { get; private set; }
    [field: SerializeField] public EventReference playerDash { get; private set; }
    [field: SerializeField] public EventReference playerLevelUp { get; private set; }

    // Enemies

    [field: Header("Grunt SFX")]
    [field: SerializeField] public EventReference gruntFootsteps { get; private set; }
    [field: SerializeField] public EventReference gruntAttack { get; private set; }
    [field: SerializeField] public EventReference gruntAlert { get; private set; }
    [field: SerializeField] public EventReference gruntDeath { get; private set; }

    [field: Header("Speedster SFX")]
    [field: SerializeField] public EventReference speedsterFootsteps { get; private set; }
    [field: SerializeField] public EventReference speedsterAttack { get; private set; }
    [field: SerializeField] public EventReference speedsterAlert { get; private set; }
    [field: SerializeField] public EventReference speedsterDeath { get; private set; }

    [field: Header("Bruiser SFX")]
    [field: SerializeField] public EventReference bruiserFootsteps { get; private set; }
    [field: SerializeField] public EventReference bruiserAttack { get; private set; }
    [field: SerializeField] public EventReference bruiserAlert { get; private set; }
    [field: SerializeField] public EventReference bruiserDeath { get; private set; }

    [field: Header("Summoner SFX")]
    [field: SerializeField] public EventReference summonerSummon { get; private set; }
    [field: SerializeField] public EventReference summonerAttack { get; private set; }
    [field: SerializeField] public EventReference summonerDeath { get; private set; }

    [field: Header("Boss SFX")]
    [field: SerializeField] public EventReference bossDeath { get; private set; }
    [field: SerializeField] public EventReference bossAttackMelee { get; private set; }
    [field: SerializeField] public EventReference bossAttackRanged { get; private set; }
    [field: SerializeField] public EventReference bossWake { get; private set; }
    [field: SerializeField] public EventReference bossShellOpen { get; private set; }
    [field: SerializeField] public EventReference bossTheme { get; private set; }
    [field: SerializeField] public EventReference bossFootsteps { get; private set; }

    // Perks and Items

    [field: Header("Perk SFX")]
    [field: SerializeField] public EventReference perkSelect { get; private set; }
    [field: SerializeField] public EventReference perkTriggerGeneric { get; private set; }
    [field: SerializeField] public EventReference itemPickup { get; private set; }
    [field: SerializeField] public EventReference itemActivate { get; private set; }

    // UI and UX

    [field: Header("UI SFX")]
    [field: SerializeField] public EventReference uiClick { get; private set; }
    [field: SerializeField] public EventReference uiHover { get; private set; }
    [field: SerializeField] public EventReference uiBack { get; private set; }
    [field: SerializeField] public EventReference uiConfirm { get; private set; }
    [field: SerializeField] public EventReference uiError { get; private set; }

    // Music and Ambience

    [field: Header("Music")]
    [field: SerializeField] public EventReference mainMenuTheme { get; private set; }
    [field: SerializeField] public EventReference levelAmbience { get; private set; }
    [field: SerializeField] public EventReference victoryStinger { get; private set; }
    [field: SerializeField] public EventReference defeatStinger { get; private set; }

    // Singleton

    public static FMODEvents Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            UnityEngine.Debug.Log("Found more than one FMOD Events instance in the scene");
        }
        else
        {
            Instance = this;
        }
    }
}