using UnityEngine;

public class BossLavaZone : MonoBehaviour
{
    [SerializeField] private float damagePerSecond = 10f;
    [SerializeField] private LayerMask playerLayer;

    private Transform bossTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bossTransform = GetComponentInParent<Transform>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = new Vector3(bossTransform.position.x, transform.position.y, transform.position.z);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        {
            //if (((1 << other.gameObject.layer) & playerLayer) == 0) return;
            other.GetComponent<PlayerMovement>()?.DamagePlayer(damagePerSecond * Time.deltaTime);
        }
    }
}
