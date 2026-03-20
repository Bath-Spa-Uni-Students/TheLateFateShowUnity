using System.Collections;
using UnityEngine;
public class BeetleBoss : MonoBehaviour, IDamageable
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask wallLayer;
    [Tooltip("trigger collider in front of boss, enabled only during slam defense.")]
    [SerializeField] private Collider2D frontDefenseTrigger;

    [Header("Health / Phase")]
    [SerializeField] private float maxHealth = 200f;
    [SerializeField, Range(0f, 1f)] private float phase2HealthThreshold = 0.5f;
    private float hp;
    private bool phase2Unlocked;
    private bool isDead;

    [Header("Facing")]
    [Tooltip("How fast the boss rotates to face the player.")]
    [SerializeField] private float faceTurnSpeed = 12f;
    [Tooltip("If your sprite faces UP instead of RIGHT, set this to (0,1). If faces RIGHT, (1,0).")]
    [SerializeField] private Vector2 bossForwardLocal = Vector2.right;

    [Header("Slam (Phase 1+)")]
    [SerializeField] private float slamTelegraphTime = 0.35f;
    [SerializeField] private float slamDefenseTime = 0.45f;
    [SerializeField] private float slamVulnTime = 0.25f;
    [SerializeField] private float slamCooldown = 2f;
    [SerializeField, Range(-1f, 1f)] private float frontBlockDotThreshold = 0.2f;

    [Header("Crystal Beam (Phase 2)")]
    [SerializeField] private float beamTelegraphTime = 0.4f;
    [SerializeField] private float beamDuration = 2.5f;
    [SerializeField] private float beamCooldown = 3f;
    [SerializeField] private int beamCount = 8;
    [SerializeField] private int beamLayers = 2;
    [SerializeField] private float beamAngularThickness = 8f;
    [SerializeField] private float beamRotateSpeedDeg = 25f;
    [SerializeField] private float beamDamage = 10f;
    [SerializeField] private float beamHitCooldown = 0.25f;

    private bool frontDefenseActive;
    private bool vulnerableWindow;

    private Coroutine combatRoutine;
    private float lastBeamHitTime = -999f;

    private void Awake()
    {
        hp = maxHealth;
    }

    private void Start()
    {
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        if (frontDefenseTrigger != null)
            frontDefenseTrigger.enabled = false;

        combatRoutine = StartCoroutine(CombatLoop());
    }

    private void Update()
    {
        if (isDead || player == null) return;

        Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        float targetAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;

        // Rotate boss to face player (2D)
        Quaternion targetRot = Quaternion.AngleAxis(targetAngle, Vector3.forward);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * faceTurnSpeed);
    }

    private IEnumerator CombatLoop()
    {
        float nextSlam = Time.time;
        float nextBeam = Time.time;

        while (!isDead && hp > 0f)
        {
            if (phase2Unlocked && Time.time >= nextBeam)
            {
                yield return CrystalBeamAttack();
                if (isDead) yield break;
                nextBeam = Time.time + beamCooldown;
                nextSlam = Time.time + slamCooldown * 0.7f;
                continue;
            }

            if (Time.time >= nextSlam)
            {
                yield return SlamAttack();
                if (isDead) yield break;
                nextSlam = Time.time + slamCooldown;
                continue;
            }

            yield return null;
        }
    }

    private IEnumerator SlamAttack()
    {
        vulnerableWindow = false;
        SetFrontDefenseCollider(false);
        frontDefenseActive = false;

        // Telegraph (hook Animator here)
        yield return new WaitForSeconds(slamTelegraphTime);

        // Defense window: front hits blocked
        SetFrontDefenseCollider(true);
        frontDefenseActive = true;
        yield return new WaitForSeconds(slamDefenseTime);

        SetFrontDefenseCollider(false);
        frontDefenseActive = false;

        // Short flank window
        vulnerableWindow = true;
        yield return new WaitForSeconds(slamVulnTime);
        vulnerableWindow = false;
    }

    private IEnumerator CrystalBeamAttack()
    {
        vulnerableWindow = false;
        SetFrontDefenseCollider(false);
        frontDefenseActive = false;

        // Telegraph (hook crystal glow here)
        yield return new WaitForSeconds(beamTelegraphTime);

        float end = Time.time + beamDuration;
        while (Time.time < end && !isDead)
        {
            TryBeamHitPlayer();
            yield return new WaitForFixedUpdate();
        }
    }

    private void TryBeamHitPlayer()
    {
        if (player == null) return;
        if (Time.time - lastBeamHitTime < beamHitCooldown) return;

        Vector2 bossPos = transform.position;
        Vector2 toPlayer = (Vector2)player.position - bossPos;
        float dist = toPlayer.magnitude;
        if (dist < 0.01f) return;

        Vector2 dirToPlayer = toPlayer / dist;

        // Wall blocks damage if something is between boss and player
        RaycastHit2D block = Physics2D.Raycast(bossPos, dirToPlayer, dist, wallLayer);
        if (block.collider != null) return;

        float rotationOffset = Time.time * beamRotateSpeedDeg;
        Vector2 baseDir = Rotate(bossForwardLocal.normalized, rotationOffset);
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;

        float hitWidth = beamAngularThickness;

        for (int layer = 0; layer < beamLayers; layer++)
        {
            float layerOffset = (360f / Mathf.Max(1, beamCount)) * (layer * 0.18f);

            for (int i = 0; i < beamCount; i++)
            {
                float angle = baseAngle + i * (360f / Mathf.Max(1, beamCount)) + layerOffset;
                Vector2 beamDir = AngleToDir(angle);

                float angDiff = Vector2.Angle(beamDir, dirToPlayer);
                if (angDiff > hitWidth) continue;

                lastBeamHitTime = Time.time;

                var stats = player.GetComponent<PlayerStats>();
                if (stats != null)
                    stats.DamagePlayer(beamDamage);

                return;
            }
        }
    }

    public void TakeDamage(float amount, Vector2 hitPoint, Vector2 attackerPosition)
    {
        if (isDead) return;

        // Front defense during slam
        if (frontDefenseActive)
        {
            Vector2 bossToAttacker = attackerPosition - (Vector2)transform.position;
            if (bossToAttacker.sqrMagnitude > 0.0001f)
            {
                Vector2 attackerDir = bossToAttacker.normalized;
                Vector2 bossForward = transform.rotation * new Vector3(bossForwardLocal.x, bossForwardLocal.y, 0f);
                bossForward.Normalize();

                float dot = Vector2.Dot(bossForward, attackerDir);
                if (dot > frontBlockDotThreshold)
                    return;
            }
        }

        hp -= amount;

        if (!phase2Unlocked && hp <= maxHealth * phase2HealthThreshold)
            phase2Unlocked = true;

        if (hp <= 0f)
        {
            hp = 0f;
            isDead = true;
            rbStop();
            SetFrontDefenseCollider(false);
            frontDefenseActive = false;
            vulnerableWindow = false;
            if (combatRoutine != null) StopCoroutine(combatRoutine);
        }

        // hitPoint available for VFX / hit flash
        _ = hitPoint;
    }

    private void rbStop()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    private void SetFrontDefenseCollider(bool on)
    {
        if (frontDefenseTrigger != null)
            frontDefenseTrigger.enabled = on;
    }

    private static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float c = Mathf.Cos(rad);
        float s = Mathf.Sin(rad);
        return new Vector2(c * v.x - s * v.y, s * v.x + c * v.y);
    }

    private static Vector2 AngleToDir(float angleDeg)
    {
        float r = angleDeg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(r), Mathf.Sin(r));
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, player.position);
    }
}