using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    //Weapon overlaps with player
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("fart");
            Destroy(gameObject);

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.GetComponent<Weapon>().canShoot = true;
        }
            
    }
}