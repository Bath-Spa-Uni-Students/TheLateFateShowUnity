using UnityEngine;

public class Weapon : MonoBehaviour
{
    //What is being shot
    public GameObject bulletPrefab;
    //Where is it being shot
    public Transform firePoint;
    //Speed of bullet
    public float bulletSpeed = 0f;

    public void Shoot()
    {

        //Create copies of the bullet prefab at firepoint location
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        //Adding force to the bullet so it moves in the direction
        bullet.GetComponent<Rigidbody2D>().AddForce(firePoint.up * bulletSpeed, ForceMode2D.Impulse);
    }
}
