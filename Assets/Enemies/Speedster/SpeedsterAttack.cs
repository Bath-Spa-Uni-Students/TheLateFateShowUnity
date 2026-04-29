using System.Collections;
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

        if (stats == null) 
            Debug.LogWarning("EnemyStats not found on " + gameObject.name);
    }

    public void OnDashHit()
    {
        if (!canAttack || isAttacking)
            return;

        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        canAttack = false;

        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(GetAnimationLength("Attack"));

        var playerMovement = player?.GetComponent<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.DamagePlayer(stats.damage);

        isAttacking = false;
        canAttack = true;
    }

    private float GetAnimationLength(string clipName)
    {
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }

        Debug.LogWarning("Animation clip '" + clipName + "' not found, defaulting to 1 second");
        return 1f;
    }
}