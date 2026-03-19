using UnityEngine;

public class PistolPickup : MonoBehaviour
{
    //Weapon overlaps with player
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("fart");
            Destroy(gameObject);

            GameObject playerWeapon = GameObject.FindGameObjectWithTag("HeldWeapon");
            playerWeapon.GetComponent<Pistol>().canShoot = true;

            GameObject Player = GameObject.FindGameObjectWithTag("Player");
            Player.GetComponent<PlayerMovement>().hasWeapon = true;
        }
    }
}