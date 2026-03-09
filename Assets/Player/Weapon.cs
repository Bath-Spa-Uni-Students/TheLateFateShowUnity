using UnityEngine;

public class Weapon : MonoBehaviour
{
    // Reference main camera
    private Camera mainCam;
    // Store mouse position
    private Vector3 mousePos;

    public GameObject bullet;
    public Transform bulletTransform;
    public bool canShoot;
    private float timer;
    public float timeBetweenFiring;

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

        if(!canShoot)
        {
            timer += Time.deltaTime;

            if (timer > timeBetweenFiring)
            {
                canShoot = true;
                timer = 0;
            }
        }

        if (Input.GetMouseButtonDown(0) && canShoot)
        {
            canShoot = false;
            Instantiate(bullet, bulletTransform.position, Quaternion.identity);
        }
    }
}
