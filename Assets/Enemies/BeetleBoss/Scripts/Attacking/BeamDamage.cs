using UnityEngine;

public class BeamDamage : MonoBehaviour
{
    [SerializeField] private float damagePerSecond = 8f;
    [SerializeField] private LayerMask playerLayer;

    private bool isLive = false;   // true only during the active fire phase
    private EnemyStats bossStats;

    private void Awake()
    {
        bossStats = GetComponentInParent<EnemyStats>();
    }
    public void SetLive(bool live)
    {
        isLive = live;
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isLive) return;
        if (((1 << other.gameObject.layer) & playerLayer) == 0) return;

        var playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.DamagePlayer(damagePerSecond * Time.fixedDeltaTime);
    }
}
