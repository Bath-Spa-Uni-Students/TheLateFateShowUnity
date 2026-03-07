using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] public float health;
    [SerializeField] public float walkSpeed;

    [SerializeField] private PlayerMovement playerMovement;

    public bool isInvulnerable;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PlayerDie()
    {
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
            health -= damage;
        }
    }
}
