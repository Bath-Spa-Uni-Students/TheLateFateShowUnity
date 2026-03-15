using TMPro;
using UnityEngine;

public class AmmoCount : MonoBehaviour
{
    public Weapon weapon;
    public TextMeshProUGUI text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateAmmo();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAmmo();
    }

    private void UpdateAmmo()
    {
        text.text = $"{weapon.maxAmmo}";
    }
}
