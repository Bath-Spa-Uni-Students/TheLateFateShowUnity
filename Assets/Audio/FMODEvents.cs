using UnityEngine;
using FMODUnity;
public class FMODEvents : MonoBehaviour
{

    [field: Header("Pistol SFX")]
    [field: SerializeField] public EventReference pistolPickup { get; private set; }
    public static FMODEvents Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            UnityEngine.Debug.Log("Found more than one FMOD Events instance in the scene");
        }
        else
        {
            Instance = this;
        }
    }
}
