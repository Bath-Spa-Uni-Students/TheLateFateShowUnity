using System.Collections;
using UnityEngine;


//This is the bosses second phase attack, the boss will fire 3 beams in a cone in front of it
//the central beam is aimed at the players current transfrom, while the other two are angled to the left and right of the central beam
//potential expansion for the beams to sweep left and right
public class BossRanged : MonoBehaviour
{

    [Header("Beam Objects")]
    [SerializeField] private GameObject[] beamObjects = new GameObject[3];
    [SerializeField] private Transform firePoint; //the crystal in the middle of the boss where the beams will originate from

    [Header("Cone Settings")]
    [SerializeField] private float coneHalfAngle = 25f;// The half angle of the cone in degrees (e.g., 25 means a total cone angle of 50 degrees) smaller value means narrower cone

    [Header("Timing")]
    [SerializeField] private float telegraphDuration = 0.6f;// Time the telegraph (warning) is shown before the beams fire
    [SerializeField] private float fireDuration = 1.2f;// Time the beams are active and can damage the player

    private BeamVisual[] beamVisuals;
    private BeamDamage[] beamDamages;

    private EnemyStats stats;
    private BossBehaviour boss;
    private bool isAttacking = false;
    private Coroutine attackCoroutine;

    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
        boss = GetComponent<BossBehaviour>();

        if (stats == null)
            Debug.LogWarning("[BossRanged] EnemyStats not found.");

        // Cache per-beam components
        beamVisuals = new BeamVisual[beamObjects.Length];
        beamDamages = new BeamDamage[beamObjects.Length];

        for (int i = 0; i < beamObjects.Length; i++)
        {
            if (beamObjects[i] == null) continue;
            beamVisuals[i] = beamObjects[i].GetComponent<BeamVisual>();
            beamDamages[i] = beamObjects[i].GetComponent<BeamDamage>();
        }

        SetBeamsActive(false);

    }

    // Called every FixedUpdate while the boss is in Ranged state.
    // Starts a new attack cycle if one isn't already running.
    public void FireConeBeams()
    {
        if (isAttacking) return;
        attackCoroutine = StartCoroutine(ConeAttackCoroutine());
    }

    public void StopBeam()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        isAttacking = false;
        SetBeamsActive(false);
        SetBeamsDamaging(false);

    }

    private IEnumerator ConeAttackCoroutine()
    {
        isAttacking = true;

        //  1. Aim at the player's current position
        AimBeamsAtPlayer();

        //  2. Telegraph phase: show beams (different colour )
        SetBeamsTelegraph();
        SetBeamsDamaging(false);
        SetBeamsActive(true);

        yield return new WaitForSeconds(telegraphDuration);
        // 3. Fire phase: beams are now damaging (active cololur)
        SetBeamsActiveFire();
        SetBeamsDamaging(true);
   
        yield return new WaitForSeconds(fireDuration);

        SetBeamsActive(true);
        SetBeamsDamaging(false);

        // 4.start cooldown
        yield return new WaitForSeconds(stats != null ? stats.fireCooldown : 1.5f);
        isAttacking = false;
    }

    // Rotates the firePoint (and consequently all child beams) so the centre
    // beam points toward the player. The two outer beams are already offset
    // in local space via their own rotations set in the Inspector.
    private void AimBeamsAtPlayer()
    {
        if (boss == null || boss.Player == null) return;

        Vector2 toPlayer = (Vector2)(boss.Player.position - transform.position);
        float baseAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;

        // Centre beam at player; outer beams fanned by coneHalfAngle
        float[] angles = { baseAngle, baseAngle + coneHalfAngle, baseAngle - coneHalfAngle };

        for (int i = 0; i < beamObjects.Length && i < angles.Length; i++)
        {
            if (beamObjects[i] != null)
                beamObjects[i].transform.rotation = Quaternion.Euler(0f, 0f, angles[i] - 90f);
            // -90 because BeamVisual draws along local +Y
        }

    }

    private void SetBeamsActive(bool active)
    {
        foreach (var beam in beamObjects)
        {
            if (beam != null)
                beam.SetActive(active);// Set each beam's active state
        }
    }

    private void SetBeamsTelegraph()
    {
        foreach (var bv in beamVisuals)
            if (bv != null) bv.SetTelegraphState();
    }

    private void SetBeamsActiveFire()
    {
        foreach (var bv in beamVisuals)
            if (bv != null) bv.SetActive();
    }

    private void SetBeamsDamaging(bool live)
    {
        foreach (var bd in beamDamages)
            if (bd != null) bd.SetLive(live);
    }


}