using System.Net.Sockets;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    //What we are shooting
    public GameObject bullet;
    //Where it is being shot
    public Transform firePoint;
    //Bullet speed
    public float bulletSpeed = 10f;

    //Can the player shoot?
    public bool canShoot = false;

    //Updates every frame
    void Update()
    {
        //If the player can shoot and MB1 is clicked
        if (canShoot && Input.GetMouseButtonDown(0))
        {
            //Shoot
            Shoot();
        }
    }

    void Shoot()
    {
    }
}