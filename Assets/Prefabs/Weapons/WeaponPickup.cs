using Unity.Cinemachine;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    //Sets if the player is in range to false
    bool inRange = false;
    
    //Updates every frame
    private void Update()
    {   
        //Checks if the player is in range and has pressed E
        if (inRange && Input.GetKeyDown(KeyCode.E))
        {

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.GetComponent<PlayerShoot>().canShoot = true;

            //Destroys the object
            Destroy(gameObject);
        }
    }

    //Weapon overlaps with player
    private void OnTriggerEnter2D(Collider2D other)
    {
        //Checks if the overlapped target is the player
        if (other.CompareTag("Player"))
        {
            //Range is true
            inRange = true;
        }
    }

    //Weapon does not overlap with player
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            //Range is false
            inRange = false;
        }
    }
}
