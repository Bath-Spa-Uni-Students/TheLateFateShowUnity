using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// Attach this to a trigger collider placed at the midpoint on the boss.
// When the player enters it they are teleported back in front of the boss
// and the host provides some excuse as for why

public class BossTeleportBarrier : MonoBehaviour
{
    [Header("Boss Reference")]
    [SerializeField] private BossBehaviour bossBehaviour;

    [Header("Quip UI")]
    [SerializeField] private GameObject quipPanel;
    [SerializeField] private TMPro.TextMeshProUGUI quipText;
    [SerializeField] private float quipDuration = 3f;

    private static readonly string[] quips = new string[]
    {
        "\"Due to budget cuts, I regret to inform the boss can't turn around…\"",
    };

    private string playerTag = "Player";
    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (bossBehaviour == null)
            bossBehaviour = GetComponentInParent<BossBehaviour>();

        if (bossBehaviour == null)
            Debug.LogWarning("[BossTriggerBarrier] No BossBehaviour found. Teleport will still work but no front-point reference.");

        if (quipPanel != null)
            quipPanel.SetActive(false);
    }
    // When the player enters the trigger, they are teleported to a point in front of the boss and a quip is displayed.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        TeleportPlayerToFront(other.transform);
        ShowQuip();
    }
    // Teleports the player to a point in front of the boss. If BossBehaviour is available, it uses its method; otherwise, it defaults to a position slightly below the barrier's parent.
    private void TeleportPlayerToFront(Transform playerTransform)
    {
        Vector3 destination;

        if (bossBehaviour != null)
        {
            destination = bossBehaviour.GetPointInFront();
        }
        else
        {
            destination = transform.parent != null
                ? transform.parent.position + Vector3.down * 1.5f
                : transform.position + Vector3.down * 1.5f;
        }

        destination.z = playerTransform.position.z;
        playerTransform.position = destination;

        var playerRb = playerTransform.GetComponent<Rigidbody2D>();
        if (playerRb != null)
            playerRb.linearVelocity = Vector2.zero;
    }

    private void ShowQuip()
    {
        if (quipPanel == null || quipText == null) return;

        quipText.text = quips[Random.Range(0, quips.Length)];
        quipPanel.SetActive(true);

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(quipDuration);
        if (quipPanel != null)
            quipPanel.SetActive(false);
    }
}