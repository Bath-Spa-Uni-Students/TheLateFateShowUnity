using UnityEngine;

public class Pistol : MonoBehaviour
{
    // Reference mouse position
    private Vector3 mousePos;

    // Reference main camera
    private Camera mainCam;

    // Bullet object
    [SerializeField] private GameObject bullet;
    private GameObject shotBullet;

    // Where bullet is being shot
    [SerializeField] private Transform bulletTransform;

    // Can the player shoot
    public bool canShoot = false;

    // Max ammo
    [SerializeField] int maxAmmo = 6;

    // Clip size
    public int ammo = 6;

    // Start is called before the first frame update
    void Start()
    {
        // Get camera component
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    private void Update()
    {
        // Get mouse position
        mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        // Calculate direction of mouse
        Vector3 rotation = mousePos - transform.position;

        // Gets angle
        float rotateZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;

        // Rotate Z axis
        transform.rotation = Quaternion.Euler(0, 0, rotateZ);

        // Player shoots
        if (Input.GetMouseButtonDown(0) && canShoot == true)
        {
            ammo -= 1;

            // Cannot shoot if ammo is 0
            if (ammo <= 0)
            {
                canShoot = false;
            }
            // Spawns bullet 
            shotBullet = Instantiate(bullet, bulletTransform.position, bulletTransform.rotation);
            shotBullet.GetComponent<PistolBullet>().pistol = gameObject.GetComponent<Pistol>();
        }

        // Reload pistol
        GameObject Player = GameObject.FindGameObjectWithTag("Player");
        if (Player.GetComponent<PlayerMovement>().hasWeapon == true && Input.GetKeyDown(KeyCode.R))
        {
            canShoot = true;
            ammo = maxAmmo;
        }
    }
}