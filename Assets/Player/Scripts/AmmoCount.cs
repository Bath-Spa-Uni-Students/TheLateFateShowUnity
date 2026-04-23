using TMPro;
using UnityEngine;

public class AmmoCount : MonoBehaviour
{
    public Pistol pistol;
    public Shotgun shotgun;
    public GameObject player;
    public TextMeshProUGUI text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void UpdateAmmo(int gunAmmo)
    {
            text.text = $"{gunAmmo}";
    }
}
