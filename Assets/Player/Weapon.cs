using UnityEngine;

public class Weapon : MonoBehaviour
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
    public bool canShoot;

    // Internal cooldown
    private float timer;

    // Firerate
    public float fireRate;

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

        // Can the player shoot
        if(!canShoot)
        {
            // Timer counts up per frame
            timer += Time.deltaTime;

            // Timer greater than firerate?
            if (timer > fireRate)
            {
                // Can shoot
                canShoot = true;
                timer = 0;
            }
        }

        // Player shoots
        if (Input.GetMouseButtonDown(0) && canShoot)
        {
            canShoot = false;

            // Spawns bullet 
            Instantiate(bullet, bulletTransform.position, Quaternion.identity);
        }
    }
}
