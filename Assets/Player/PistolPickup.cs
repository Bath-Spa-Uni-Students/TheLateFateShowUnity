using UnityEngine;
using FMODUnity;

public class PistolPickup : MonoBehaviour
{

    [SerializeField] private EventReference pistolPickupSound;

    //Weapon overlaps with player
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);

            GameObject playerWeapon = GameObject.FindGameObjectWithTag("HeldWeapon");
            playerWeapon.GetComponent<Pistol>().enabled = true;
            playerWeapon.GetComponent<Shotgun>().enabled = false;
            playerWeapon.GetComponent<Pistol>().canShoot = true;

            GameObject Player = GameObject.FindGameObjectWithTag("Player");
            Player.GetComponent<PlayerMovement>().hasWeapon = true;

            AudioManager.Instance.playoneShot(pistolPickupSound, this.transform.position);
        }
    }

}