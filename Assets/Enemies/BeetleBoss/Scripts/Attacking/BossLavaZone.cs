using System.Collections;
using UnityEngine;

public class BossLavaZone : MonoBehaviour
{
    [SerializeField] private float damagePerSecond = 10f;
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private LayerMask playerLayer;

    private bool isPlayerInLava = false;
    private PlayerMovement playerHealthScript;
    private float playerHealth;

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

    private void OnTriggerEnter2D(Collider2D player)
    {
        if (player.CompareTag("Player"))
        {
            playerHealthScript = player.GetComponent<PlayerMovement>();
            playerHealth = playerHealthScript.health;
            isPlayerInLava = true;
            StartCoroutine(LavaDamageCoroutine(player));
        }
    }

    private void OnTriggerExit2D(Collider2D player)
    {
        if (player.CompareTag("Player"))
        {
            isPlayerInLava = false;
            StopCoroutine(LavaDamageCoroutine(player));
        }
    }

    IEnumerator LavaDamageCoroutine(Collider2D player)
    {
        while (isPlayerInLava)
        {
            if (playerHealth != null)
            {
                playerHealthScript.DamagePlayer(damagePerSecond);
            }
            yield return new WaitForSeconds(damageInterval);
        }
    }

}
