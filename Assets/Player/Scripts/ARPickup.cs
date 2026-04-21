using UnityEngine;

public class ARPickup : MonoBehaviour
{


    //Weapon overlaps with player
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Destroy(gameObject);

            GameObject ARWeapon = GameObject.FindGameObjectWithTag("ARHeld");
            // AR script enabled
            ARWeapon.GetComponent<AR>().enabled = true;
            // Enable AR sprite
            ARWeapon.GetComponent<SpriteRenderer>().enabled = true;
            // AR can shoot
            ARWeapon.GetComponent<AR>().canShoot = true;

            GameObject Player = GameObject.FindGameObjectWithTag("Player");
            // Player has a weapon
            Player.GetComponent<PlayerMovement>().hasWeapon = true;

            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.weaponPickup, this.transform.position);
        }
    }

}