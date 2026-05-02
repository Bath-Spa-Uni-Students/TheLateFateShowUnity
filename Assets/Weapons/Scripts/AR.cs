using UnityEngine;
using UnityEngine.UI;
using FMOD.Studio;

public class AR : MonoBehaviour
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
    [SerializeField] int maxAmmo = 15;

    //Fire rate
    [SerializeField] public float fireRate = 0.2f; // seconds between shots
    private float nextFireTime = 0f;

    // Clip size
    public int ammo = 10;
    [SerializeField] private GameObject ammoText;

    //Audio
    private EventInstance ARShoot;
    private EventInstance ARReload;
    private EventInstance ARNoAmmo;

    // Start is called before the first frame update
    void Start()
    {
        // Get camera component
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        ammoText.gameObject.GetComponent<AmmoCount>().UpdateAmmo(ammo);
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
        if (Input.GetMouseButton(0) && canShoot == true && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            ammo -= 1;
            ammoText.gameObject.GetComponent<AmmoCount>().UpdateAmmo(ammo);

            // Cannot shoot if ammo is 0

            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.arShoot, transform.position);

            if (ammo <= 0)
            {

                canShoot = false;
            }
            // Spawns bullet 
            shotBullet = Instantiate(bullet, bulletTransform.position, bulletTransform.rotation);
            shotBullet.GetComponent<ARBullet>().ar = gameObject.GetComponent<AR>();
        }
        else if (Input.GetMouseButtonDown(0) && canShoot == false)
        {
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.arNoAmmo, transform.position);
        }

        // Reload AR
        GameObject Player = GameObject.FindGameObjectWithTag("Player");
        if (Player.GetComponent<PlayerMovement>().hasWeapon == true && Input.GetKeyDown(KeyCode.R))
        {
            canShoot = true;
            ammo = maxAmmo;
            ammoText.gameObject.GetComponent<AmmoCount>().UpdateAmmo(ammo);
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.pistolReload, transform.position);
        }
    }
}