using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SpeedsterAttack : MonoBehaviour
{
    private EnemyStats stats;
    private Rigidbody2D rb;
    private Transform player;
    private Animator animator;

    [SerializeField] private bool canAttack = true;
    [SerializeField] private bool isAttacking = false;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator = GetComponent<Animator>();
        stats = GetComponent<EnemyStats>(); 

        if (stats != null)
        {
            Debug.LogWarning("EnemyStats not found on " + gameObject.name);
        }
    }
   public void OnDashHit()
    {
        if (!canAttack || isAttacking) 
        {
            return;
        }
        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        canAttack = false;


       //Deal damage to player
       var playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.DamagePlayer(stats.damage);
        }

        yield return new WaitForSeconds(1f); // Attack cooldown
    }
}