using UnityEngine;

public class GunUI : MonoBehaviour
{
    public static GunUI Instance;
    public GameObject pistolUI;
    public GameObject shotgunUI;
    public GameObject arUI;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        WeaponManager.Instance.OnWeaponChanged += OnWeaponChanged;
        // Set initial state
        OnWeaponChanged(WeaponManager.Instance.CurrentWeapon);
    }

    private void OnDestroy()
    {
        if (WeaponManager.Instance != null)
            WeaponManager.Instance.OnWeaponChanged -= OnWeaponChanged;
    }

    private void OnWeaponChanged(WeaponInstance weapon)
    {
        pistolUI.SetActive(weapon.weaponType == WeaponType.Pistol);
        shotgunUI.SetActive(weapon.weaponType == WeaponType.Shotgun);
        arUI.SetActive(weapon.weaponType == WeaponType.AR);
    }
}