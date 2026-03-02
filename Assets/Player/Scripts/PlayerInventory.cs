using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private GameObject equippedWeapon;
    [SerializeField] private GameObject weaponSlot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Interact(InputAction.CallbackContext context)
    {
        EquipWeapon(equippedWeapon);
    }

    public void EquipWeapon (GameObject weapon)
    {
        weaponSlot.gameObject.SetActive (true);
        equippedWeapon = weapon;
        Debug.Log("Weapon Equip");
    }
}
