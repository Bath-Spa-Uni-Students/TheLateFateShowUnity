using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// Attach this to a trigger collider placed BEHIND the boss.
// When the player enters it they are teleported back in front of the boss
// and the host provides some excuse as for why

public class BossTeleportBarrier : MonoBehaviour
{

    [Header("Boss Reference")]
    [SerializeField] private BossBehaviour bossBehaviour;

    [Header("Quip UI")]
    [SerializeField] private GameObject quipPanel;// A UI panel that contains the quip text, which can be enabled/disabled
    [SerializeField] private Text quipText;// UI elements to display the quip
    [SerializeField] private float quipDuration = 3f;// How long the quip is shown before disappearing

    // A pool of lines to randomly pick from
    private static readonly string[] quips = new string[]
    {
        "\"Due to budget cuts, I regret to inform the boss can't turn around…\"",
    };

    // Runtime
    private string playerTag = "Player";
    private Coroutine hideCoroutine;
    private Canvas fallbackCanvas;   // created at runtime if no quipPanel is wired up

    private void Awake()
    {
        // Auto-find BossBehaviour on parent if not assigned
        if (bossBehaviour == null)
            bossBehaviour = GetComponentInParent<BossBehaviour>();

        if (bossBehaviour == null)
            Debug.LogWarning("[BossTriggerBarrier] No BossBehaviour found. Teleport will still work but no front-point reference.");

      

        if (quipPanel != null)
            quipPanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //teleport player back in front of the boss
        if (!other.CompareTag(playerTag)) return;

        TeleportPlayerToFront(other.transform);
        //ShowQuip();
    }

    private void TeleportPlayerToFront(Transform playerTransform)
    {
        Vector3 destination;

        if (bossBehaviour != null)
        {
            destination = bossBehaviour.GetPointInFront();
        }
        else
        {
            // Fallback: put player directly below the parent transform
            destination = transform.parent != null
                ? transform.parent.position + Vector3.down * 1.5f
                : transform.position + Vector3.down * 1.5f;
        }

        destination.z = playerTransform.position.z;
        playerTransform.position = destination;

        // Zero out the player velocity so they don't slide through
        var playerRb = playerTransform.GetComponent<Rigidbody2D>();
        if (playerRb != null)
            playerRb.linearVelocity = Vector2.zero;

    }

    //ui stuff here, show a random quip from the pool for a few seconds when the player hits the barrier


}
