using UnityEngine;

public class PistolPickup : MonoBehaviour
{
    //Weapon overlaps with player
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);

            GameObject playerWeapon = GameObject.FindGameObjectWithTag("HeldWeapon");
            // Pistol script enabled
            playerWeapon.GetComponent<Pistol>().enabled = true;
            // Enable pistol sprite
            playerWeapon.GetComponent<SpriteRenderer>().enabled = true;
            // Shotgun script disabled (Player can not shoot the pistol and shotgun at the same time)
            playerWeapon.GetComponent<Shotgun>().enabled = false;
            // Pistol can shoot
            playerWeapon.GetComponent<Pistol>().canShoot = true;

            GameObject Player = GameObject.FindGameObjectWithTag("Player");
            // Player has a weapon
            Player.GetComponent<PlayerMovement>().hasWeapon = true;
        }
    }
}