using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{

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

    [field: Header("Player SFX")]
    [field: SerializeField] public EventReference playerFootsteps { get; private set; }
    [field: SerializeField] public EventReference playerHurt { get; private set; }
    [field: SerializeField] public EventReference playerDeath { get; private set; }
    [field: SerializeField] public EventReference playerDash { get; private set; }
    [field: SerializeField] public EventReference playerLevelUp { get; private set; }

    [field: Header("Grunt SFX")]
    [field: SerializeField] public EventReference gruntFootsteps { get; private set; }
    [field: SerializeField] public EventReference gruntAttack { get; private set; }
    [field: SerializeField] public EventReference gruntAlert { get; private set; }
    [field: SerializeField] public EventReference gruntDeath { get; private set; }
    [field: SerializeField] public EventReference gruntTakeDamage { get; private set; }
    [field: Header("Speedster SFX")]
    [field: SerializeField] public EventReference speedsterFootsteps { get; private set; }
    [field: SerializeField] public EventReference speedsterAttack { get; private set; }
    [field: SerializeField] public EventReference speedsterAlert { get; private set; }
    [field: SerializeField] public EventReference speedsterDeath { get; private set; }
    [field: SerializeField] public EventReference speedsterTakeDamage { get; private set; }
    [field: Header("Boss SFX")]
    [field: SerializeField] public EventReference bossDeath { get; private set; }
    [field: SerializeField] public EventReference bossAttackMelee { get; private set; }
    [field: SerializeField] public EventReference bossAttackRanged { get; private set; }
    [field: SerializeField] public EventReference bossWake { get; private set; }
    [field: SerializeField] public EventReference bossShellOpen { get; private set; }
    [field: SerializeField] public EventReference bossFootsteps { get; private set; }
    [field: SerializeField] public EventReference bossTakeDamage { get; private set; }
    [field: SerializeField] public EventReference bossPhaseChange { get; private set; }

    [field: Header("Perk SFX")]
    [field: SerializeField] public EventReference perkSelect { get; private set; }
    [field: SerializeField] public EventReference perkTriggerGeneric { get; private set; }
    [field: SerializeField] public EventReference itemPickup { get; private set; }
    [field: SerializeField] public EventReference itemActivate { get; private set; }

    [field: Header("UI SFX")]
    [field: SerializeField] public EventReference uiPause { get; private set; }
    [field: SerializeField] public EventReference uiHover { get; private set; }
    [field: SerializeField] public EventReference uiBack { get; private set; }
    [field: SerializeField] public EventReference uiConfirm { get; private set; }
    [field: SerializeField] public EventReference uiError { get; private set; }
    [field: SerializeField] public EventReference crowdNoise { get; private set; }
    [field: SerializeField] public EventReference crowdApplause { get; private set; }
    [field: SerializeField] public EventReference tvStatic { get; private set; }
    [field: SerializeField] public EventReference tvBeep { get; private set; }
    [field: SerializeField] public EventReference hostAppear { get; private set; }
    [field: SerializeField] public EventReference typeWriterClick { get; private set; }
    [field: SerializeField] public EventReference typeWriterDing { get; private set; }
    [field: SerializeField] public EventReference mazeChange { get; private set; }
    [field: SerializeField] public EventReference fameGain { get; private set; }

    [field: Header("Music")]
    [field: SerializeField] public EventReference mainMenuTheme { get; private set; }
    [field: SerializeField] public EventReference explorationTheme { get; private set; }
    [field: SerializeField] public EventReference bossTheme { get; private set; }
    [field: SerializeField] public EventReference victoryStinger { get; private set; }
    [field: SerializeField] public EventReference defeatStinger { get; private set; }

    [field: Header("Game Object SFX")]
    [field: SerializeField] public EventReference keyPickup { get; private set; }
    [field: SerializeField] public EventReference keyHum { get; private set; }
    [field: SerializeField] public EventReference keyMerge { get; private set; }
    [field: SerializeField] public EventReference chestOpen { get; private set; }
    [field: SerializeField] public EventReference chestHum { get; private set; }
   

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