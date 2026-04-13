using FMOD.Studio;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] public float health;
    [SerializeField] public float walkSpeed;

    [SerializeField] private PlayerMovement playerMovement;


    //audio
    private EventInstance playerHurt;
    private EventInstance playerDeath;

    public bool isInvulnerable;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerHurt = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.playerHurt);
        playerDeath = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.playerDeath);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PlayerDie()
    {
        playerDeath.start();
        Destroy(gameObject);
    }

    public void DamagePlayer(float damage)
    {
        if (health <= 0 || health - damage <= 0)
        {
            PlayerDie();
        }
        else
        {
            Debug.Log("Player took " + damage + " damage. Remaining health: " + (health - damage));
            health -= damage;
            playerHurt.start();
        }
    }
}
