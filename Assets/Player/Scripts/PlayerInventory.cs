using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private GameObject equippedWeapon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EquipWeapon (GameObject weapon)
    {
        equippedWeapon = weapon;
        
    }
}
