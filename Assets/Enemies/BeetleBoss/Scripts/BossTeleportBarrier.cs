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

    [Header("Teleport Timing")]
    [SerializeField] private float staticDuration = 0.6f;   // how long the static covers the screen
    [SerializeField] private float cooldownDuration = 2f;   // lockout after a teleport finishes

    [Header("Static Effect")]
    [SerializeField] private GameObject staticPanel;//this could also be the curtains animation but i want to save that so will use this if we can produce it in time

    private static readonly string[] quips = new string[]
    {
        "\"Due to budget cuts, I regret to inform the boss can't turn around…\"",
    };

    private string playerTag = "Player";
    private Coroutine hideCoroutine;
    private bool isCoolingDown = false;

    private void Awake()
    {
        if (bossBehaviour == null)
            bossBehaviour = GetComponentInParent<BossBehaviour>();

        if (bossBehaviour == null)
            Debug.LogWarning("[BossTriggerBarrier] No BossBehaviour found. Teleport will still work but no front-point reference.");

        if (quipPanel != null)
            quipPanel.SetActive(false);

        if (staticPanel != null)
            staticPanel.SetActive(false);
    }
    // When the player enters the trigger, they are teleported to a point in front of the boss and a quip is displayed.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (isCoolingDown) return;

        StartCoroutine(TeleportSequence(other.transform));
    }
    private IEnumerator TeleportSequence(Transform playerTransform)
    {
        isCoolingDown = true;

        // 1. Flash static on
        if (staticPanel != null)
            staticPanel.SetActive(true);

        // 2. Wait for static to cover the screen, then teleport mid-flash
        yield return new WaitForSeconds(staticDuration * 0.5f);
        TeleportPlayerToFront(playerTransform);

        // 3. Hold static for the second half
        yield return new WaitForSeconds(staticDuration * 0.5f);

        // 4. Static off, show quip
        if (staticPanel != null)
            staticPanel.SetActive(false);

        ShowQuip();

        // 5. Cooldown before the barrier can trigger again
        yield return new WaitForSeconds(cooldownDuration);
        isCoolingDown = false;
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