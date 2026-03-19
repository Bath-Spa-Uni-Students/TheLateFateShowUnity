using UnityEngine;

public class Shotgun : MonoBehaviour
{
    // Reference mouse position
    private Vector3 mousePos;

    // Reference main camera
    private Camera mainCam;

    // Bullet object
    public GameObject bullet;

    // Where bullet is being shot
    public Transform bulletTransform;

    // Can the player shoot
    public bool canShoot = false;

    // Firerate
    public float fireRate;

    // Max ammo
    public int maxAmmo = 6;

    // Pellet count
    public int pelletCount = 6;

    // Bullet spread
    public float bulletSpread = 20f;

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
            maxAmmo -= 1;

            if (maxAmmo <= 0)
            {
                canShoot = false;
            }
            // Spawns bullet 
            for (int i = 0; i < pelletCount; i++)
            {
                float spread = Random.Range(-bulletSpread, bulletSpread);
                Quaternion spreadRotation = Quaternion.Euler(0, 0, spread);

                Instantiate(bullet, bulletTransform.position, bulletTransform.rotation * spreadRotation);
            }
        }

        GameObject Player = GameObject.FindGameObjectWithTag("Player");
        if (Player.GetComponent<PlayerMovement>().hasWeapon == true && Input.GetKeyDown(KeyCode.R))
        {
            canShoot = true;
            maxAmmo = 6;
        }
    }
}
