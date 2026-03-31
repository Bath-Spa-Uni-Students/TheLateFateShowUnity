using System.Collections;
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

    private void Awake()
    {


        if (stats == null)
        {
            Debug.LogWarning("EnemyStats component not found on " + gameObject.name);
        }
    }

    public void SpinBeam()
    {
        StartCoroutine(SpinBeamCoroutine());
    }

    private IEnumerator SpinBeamCoroutine()
    {
        canShoot = false;

        float timer = 0f;

        while (timer < stats.beamSpinDuration)
        {
            spinBeam.transform.Rotate(0f, 0f, stats.beamSpinSpeed * Time.deltaTime);
            timer += Time.deltaTime;

            yield return null;
        }

        yield return new WaitForSeconds(stats.fireCooldown);

        canShoot = true;
    }
}