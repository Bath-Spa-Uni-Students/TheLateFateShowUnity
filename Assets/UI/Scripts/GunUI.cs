using UnityEngine;
using UnityEngine.UI;

public class GunUI : MonoBehaviour
{
    public static GunUI Instance;

    public GameObject pistolUI;
    public GameObject shotgunUI;
    public GameObject arUI;

    private void Awake()
    {
        Instance = this;
    }

    public void SetActiveGun(string tag)
    {
        pistolUI.SetActive(false);
        shotgunUI.SetActive(false);
        arUI.SetActive(false);

        if (tag == "PistolHeld")
            pistolUI.SetActive(true);

        else if (tag == "ShotgunHeld")
            shotgunUI.SetActive(true);

        else if (tag == "ARHeld")
            arUI.SetActive(true);
    }

    //debug key to test gun UI
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetActiveGun("PistolHeld");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetActiveGun("ShotgunHeld");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetActiveGun("ARHeld");
        }
    }
}