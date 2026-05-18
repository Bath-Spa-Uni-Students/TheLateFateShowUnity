using FMOD.Studio;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

    // Max ammo
    public int maxAmmo = 6;
    public int ammo;

    // Pellet count
    public int pelletCount = 6;
    //Fire rate
    [SerializeField] public float fireRate = 1.0f; // seconds between shots
    private float nextFireTime = 0f;

    // Bullet spread
    public float bulletSpread = 20f;

    private bool isReloading = false;

    [SerializeField] private GameObject ammoText;

    //audio
    private EventInstance shotgunShoot;
    private EventInstance shotgunReload;
    private EventInstance shotgunNoAmmo;

    // Start is called before the first frame update
    void Start()
    {
        // Get camera component
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        ammo = maxAmmo;
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
        if (Input.GetMouseButtonDown(0) && canShoot == true)
        {
            ammo -= 1;
            ammoText.gameObject.GetComponent<AmmoCount>().UpdateAmmo(ammo);

            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.shotgunShoot, transform.position);

            // Cannot shoot if ammo is 0
            if (ammo <= 0)
            {
                canShoot = false;
            }
            // Spawns spread
            for (int i = 0; i < pelletCount; i++)
            {
                //Bullet spread is random
                float spread = Random.Range(-bulletSpread, bulletSpread);
                Quaternion spreadRotation = Quaternion.Euler(0, 0, spread);

                Instantiate(bullet, bulletTransform.position, bulletTransform.rotation * spreadRotation);
            }
        }
        // If player tries to shoot with no ammo, play no ammo sound
        else if (Input.GetMouseButtonDown(0) && canShoot == false)
        {
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.shotgunNoAmmo, transform.position);
        }

            // Reload shotgun
            GameObject Player = GameObject.FindGameObjectWithTag("Player");
        if (Player.GetComponent<PlayerMovement>().hasWeapon == true && Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(ReloadTimer());
        }
    }

    private IEnumerator ReloadTimer()
    {
        if (isReloading) yield break; // Prevent multiple reloads at once
        canShoot = false;
        isReloading = true;
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.shotgunReload, transform.position); // Play reload sound
        yield return new WaitForSeconds(2f); // Simulate reload time
        ammo = maxAmmo;
        ammoText.gameObject.GetComponent<AmmoCount>().UpdateAmmo(ammo);
        isReloading = false;
        canShoot = true;
    }
}
