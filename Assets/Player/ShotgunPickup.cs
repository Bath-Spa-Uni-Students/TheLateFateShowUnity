using UnityEngine;

public class ShotgunPickup : MonoBehaviour
{
    //Weapon overlaps with player
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);

            GameObject shotgunWeapon = GameObject.FindGameObjectWithTag("ShotgunHeld");
            // Shotgun script enabled
            shotgunWeapon.GetComponent<Shotgun>().enabled = true;
            // Enable shotgun sprite
            shotgunWeapon.GetComponent<SpriteRenderer>().enabled = true;
            // Pistol script disabled (Player can not shoot the pistol and shotgun at the same time)
            shotgunWeapon.GetComponent<Shotgun>().enabled = false;
            // Shotgun can shoot
            shotgunWeapon.GetComponent<Shotgun>().canShoot = true;

            GameObject Player = GameObject.FindGameObjectWithTag("Player");
            // Player has a weapon
            Player.GetComponent<PlayerMovement>().hasWeapon = true;
        }
    }
}