using UnityEngine;

public class PistolPickup : MonoBehaviour
{
    //Weapon overlaps with player
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);

            GameObject pistolWeapon = GameObject.FindGameObjectWithTag("PistolHeld");
            // Pistol script enabled
            pistolWeapon.GetComponent<Pistol>().enabled = true;
            // Enable pistol sprite
            pistolWeapon.GetComponent<SpriteRenderer>().enabled = true;
            // Pistol can shoot
            pistolWeapon.GetComponent<Pistol>().canShoot = true;

            GameObject Player = GameObject.FindGameObjectWithTag("Player");
            // Player has a weapon
            Player.GetComponent<PlayerMovement>().hasWeapon = true;
        }
    }
}