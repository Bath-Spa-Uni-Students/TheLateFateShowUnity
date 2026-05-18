using TMPro;
using UnityEngine;
using UnityEngine.UI;
//redundant script
public class PistolPickup : MonoBehaviour
{


    //Weapon overlaps with player
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Destroy(gameObject);

            GameObject pistolWeapon = GameObject.FindGameObjectWithTag("PistolHeld");
            GameObject ammoText = GameObject.FindGameObjectWithTag("AmmoText");
            GameObject gunDisplay = GameObject.FindGameObjectWithTag("GunDisplayUI");

            // Pistol script enabled
            pistolWeapon.GetComponent<Pistol>().enabled = true;
            // Enable pistol sprite
            pistolWeapon.GetComponent<SpriteRenderer>().enabled = true;
            // Pistol can shoot
            pistolWeapon.GetComponent<Pistol>().canShoot = true;

            GameObject Player = GameObject.FindGameObjectWithTag("Player");
            // Player has a weapon
            Player.GetComponent<PlayerMovement>().hasWeapon = true;


            ammoText.GetComponent<TextMeshProUGUI>().enabled = true;

            gunDisplay.GetComponent<Image>().enabled = true;


           // GunUI.Instance.SetActiveGun("PistolHeld");
           
        }
    }

}