using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BossRanged : MonoBehaviour
{
    [SerializeField] private GameObject spinBeam;
    [SerializeField] private Transform firePoint;

    private bool canShoot = true;
    private Rigidbody2D rb;
    private Transform player;
    private Animator animator;
    private BossBehaviour bossBehaviour;
    private EnemyStats stats;
    private GameObject attackBarrier;
    private bool isAttacking;

    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
        if (stats == null)
        {
            Debug.LogWarning("EnemyStats component not found on " + gameObject.name);
        }
    }

    public void SpinBeam()
    {
        if (!isAttacking)
        {
            StartCoroutine(SpinBeamCoroutine());
        }
    }

    private IEnumerator SpinBeamCoroutine()
    {
        isAttacking = true;
        canShoot = false;

        // Enable the beam at the start of the spin
        if (spinBeam != null)
            spinBeam.SetActive(true);

        float timer = 0f;

        if (stats == null)
        {
            Debug.Log("Stats not real");
        }

        while (timer < stats.beamSpinDuration)
        {
            spinBeam.transform.Rotate(0f, 0f, stats.beamSpinSpeed * Time.deltaTime);
            timer += Time.deltaTime;

            yield return null; 
        }

        // Spin finished, now start cooldown
        if (spinBeam != null)
            spinBeam.SetActive(false); // Disable beam during cooldown

        yield return new WaitForSeconds(stats.fireCooldown);

        canShoot = true;
        isAttacking = false;
    }
}